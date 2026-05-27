# File: scripts/env/setup-env.sh
#!/usr/bin/env bash
set -euo pipefail

# Environment configuration script
# Usage: source scripts/env/setup-env.sh [environment]

ENVIRONMENT=${1:-development}

case $ENVIRONMENT in
  development)
    export AWS_PROFILE="ecommerce-dev"
    export ASPNETCORE_ENVIRONMENT="Development"
    export DOCKER_REGISTRY="localhost:5000"
    export DEPLOY_STRATEGY="rolling"
    export MIN_INSTANCES=1
    export MAX_INSTANCES=2
    export LOG_LEVEL="Debug"
    ;;

  staging)
    export AWS_PROFILE="ecommerce-staging"
    export ASPNETCORE_ENVIRONMENT="Staging"
    export DOCKER_REGISTRY="ghcr.io/your-org"
    export DEPLOY_STRATEGY="rolling"
    export MIN_INSTANCES=2
    export MAX_INSTANCES=4
    export LOG_LEVEL="Information"
    ;;

  production)
    export AWS_PROFILE="ecommerce-prod"
    export ASPNETCORE_ENVIRONMENT="Production"
    export DOCKER_REGISTRY="ghcr.io/your-org"
    export DEPLOY_STRATEGY="blue-green"
    export MIN_INSTANCES=3
    export MAX_INSTANCES=10
    export LOG_LEVEL="Warning"
    ;;

  *)
    echo "Unknown environment: $ENVIRONMENT"
    exit 1
    ;;
esac

echo "Configured environment: $ENVIRONMENT"
echo "AWS Profile: $AWS_PROFILE"
echo "ASP.NET Environment: $ASPNETCORE_ENVIRONMENT"
