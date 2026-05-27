# File: scripts/monitoring/setup-alerts.sh
#!/usr/bin/env bash
set -euo pipefail

# Set up CloudWatch alerts
# Usage: ./scripts/monitoring/setup-alerts.sh [environment]

ENVIRONMENT=${1:-development}

echo "Setting up alerts for environment: $ENVIRONMENT"

# API Availability Alert
aws cloudwatch put-metric-alarm \
  --alarm-name "ECommerce-${ENVIRONMENT}-API-Availability" \
  --alarm-description "Alert when API availability drops below 99.9%" \
  --metric-name "HTTPCode_Target_5XX_Count" \
  --namespace "AWS/ApplicationELB" \
  --statistic "Sum" \
  --period 300 \
  --threshold 5 \
  --comparison-operator "GreaterThanThreshold" \
  --evaluation-periods 2 \
  --alarm-actions "arn:aws:sns:us-east-1:${AWS_ACCOUNT_ID}:ecommerce-alerts" \
  --tags "Project=ecommerce,Environment=${ENVIRONMENT}"

# Database CPU Alert
aws cloudwatch put-metric-alarm \
  --alarm-name "ECommerce-${ENVIRONMENT}-DB-CPU" \
  --alarm-description "Alert when database CPU exceeds 80%" \
  --metric-name "CPUUtilization" \
  --namespace "AWS/RDS" \
  --statistic "Average" \
  --period 300 \
  --threshold 80 \
  --comparison-operator "GreaterThanThreshold" \
  --evaluation-periods 3 \
  --alarm-actions "arn:aws:sns:us-east-1:${AWS_ACCOUNT_ID}:ecommerce-alerts" \
  --tags "Project=ecommerce,Environment=${ENVIRONMENT}"

# Redis Memory Alert
aws cloudwatch put-metric-alarm \
  --alarm-name "ECommerce-${ENVIRONMENT}-Redis-Memory" \
  --alarm-description "Alert when Redis memory exceeds 80%" \
  --metric-name "DatabaseMemoryUsagePercentage" \
  --namespace "AWS/ElastiCache" \
  --statistic "Average" \
  --period 300 \
  --threshold 80 \
  --comparison-operator "GreaterThanThreshold" \
  --evaluation-periods 2 \
  --alarm-actions "arn:aws:sns:us-east-1:${AWS_ACCOUNT_ID}:ecommerce-alerts" \
  --tags "Project=ecommerce,Environment=${ENVIRONMENT}"

echo "Alerts setup complete for environment: $ENVIRONMENT"
