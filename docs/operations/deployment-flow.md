# Deployment Flow

## Overview
Continuous deployment pipeline using GitHub Actions and AWS CDK.

## Pipeline Stages

### 1. CI (Continuous Integration)
- Triggered on push to `develop` or `main`
- Runs linting, unit tests, integration tests
- Performs security scanning
- Builds Docker images

### 2. Infrastructure Deployment
- Triggered on merge to `develop` (staging) or `main` (production)
- CDK diff shows infrastructure changes
- Manual approval required for production

### 3. Application Deployment
- Rolling update strategy for development/staging
- Blue-green deployment for production
- Health checks verify deployment success

### 4. Post-Deployment Verification
- Smoke tests run against deployed environment
- Automated rollback on failure
- Slack notifications for deployment status

## Rollback Procedure

### Automatic Rollback
Triggered when health checks fail after deployment.

### Manual Rollback
```bash
# Rollback to previous version
aws ecs update-service \
  --cluster ecommerce-cluster-production \
  --service ecommerce-api-service \
  --task-definition ecommerce-api:previous \
  --force-new-deployment
```

## Environment URLs

Environment	API Endpoint	Frontend URL
Development	https://api.dev.ecommerce.example.com	https://dev.ecommerce.example.com
Staging	https://api.staging.ecommerce.example.com	https://staging.ecommerce.example.com
Production	https://api.ecommerce.example.com	https://ecommerce.example.com
