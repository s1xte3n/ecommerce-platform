# File: scripts/deployment/deploy.sh
#!/usr/bin/env bash
set -euo pipefail

# Complete deployment script
# Usage: ./scripts/deployment/deploy.sh [environment]

ENVIRONMENT=${1:-development}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

echo "=== Starting deployment to ${ENVIRONMENT} ==="
echo "Timestamp: $(date -u +"%Y-%m-%dT%H:%M:%SZ")"

# Load environment configuration
source "${SCRIPT_DIR}/../env/setup-env.sh" "${ENVIRONMENT}"

# Verify prerequisites
command -v aws >/dev/null 2>&1 || { echo "AWS CLI is required but not installed. Aborting."; exit 1; }
command -v docker >/dev/null 2>&1 || { echo "Docker is required but not installed. Aborting."; exit 1; }
command -v dotnet >/dev/null 2>&1 || { echo "dotnet SDK is required but not installed. Aborting."; exit 1; }

# Step 1: Build .NET Application
echo "Building .NET application..."
cd "${REPO_ROOT}/apps/api"
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --no-build

# Step 2: Build Frontend
echo "Building frontend application..."
cd "${REPO_ROOT}/apps/frontend"
npm ci
npm run build
npm test -- --ci

# Step 3: Build Docker Images
echo "Building Docker images..."
GIT_SHA=$(git rev-parse --short HEAD)

docker build \
  -t "${DOCKER_REGISTRY}/ecommerce-api:${GIT_SHA}" \
  -f "${REPO_ROOT}/apps/api/Dockerfile" \
  "${REPO_ROOT}/apps/api"

docker build \
  -t "${DOCKER_REGISTRY}/ecommerce-frontend:${GIT_SHA}" \
  -f "${REPO_ROOT}/apps/frontend/Dockerfile" \
  "${REPO_ROOT}/apps/frontend"

# Step 4: Push Images
echo "Pushing Docker images..."
docker push "${DOCKER_REGISTRY}/ecommerce-api:${GIT_SHA}"
docker push "${DOCKER_REGISTRY}/ecommerce-frontend:${GIT_SHA}"

# Step 5: Deploy Infrastructure
echo "Deploying infrastructure..."
cd "${REPO_ROOT}/infra/cdk"
npm ci
npx cdk deploy --require-approval never --context environment="${ENVIRONMENT}"

# Step 6: Update ECS Services
echo "Updating ECS services..."
aws ecs update-service \
  --cluster "ecommerce-cluster-${ENVIRONMENT}" \
  --service "ecommerce-api-service" \
  --force-new-deployment \
  --region "${AWS_REGION}"

# Step 7: Wait for Deployment
echo "Waiting for deployment to stabilize..."
aws ecs wait services-stable \
  --cluster "ecommerce-cluster-${ENVIRONMENT}" \
  --services "ecommerce-api-service" \
  --region "${AWS_REGION}"

# Step 8: Run Smoke Tests
echo "Running smoke tests..."
API_ENDPOINT=$(aws cloudformation describe-stacks \
  --stack-name "ECommerce-${ENVIRONMENT}" \
  --query 'Stacks[0].Outputs[?OutputKey==`ApiEndpoint`].OutputValue' \
  --output text)

HEALTH_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "https://${API_ENDPOINT}/health")
if [ "$HEALTH_RESPONSE" != "200" ]; then
  echo "❌ Health check failed with status: $HEALTH_RESPONSE"
  echo "Initiating rollback..."
  aws ecs update-service \
    --cluster "ecommerce-cluster-${ENVIRONMENT}" \
    --service "ecommerce-api-service" \
    --task-definition "ecommerce-api:previous" \
    --force-new-deployment \
    --region "${AWS_REGION}"
  exit 1
fi

echo "✅ Deployment to ${ENVIRONMENT} completed successfully!"
echo "API Endpoint: https://${API_ENDPOINT}"
echo "Deployment timestamp: $(date -u +"%Y-%m-%dT%H:%M:%SZ")"
echo "Git SHA: ${GIT_SHA}"
