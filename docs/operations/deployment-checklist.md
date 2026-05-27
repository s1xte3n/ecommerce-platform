# File: docs/operations/deployment-checklist.md

# Production Deployment Checklist

## Pre-Deployment
- [ ] All tests passing (unit, integration, e2e)
- [ ] Security scan completed with no critical/high issues
- [ ] Database migrations tested in staging
- [ ] Rollback plan documented and tested
- [ ] Performance benchmarks completed
- [ ] Feature flags configured (if applicable)
- [ ] Monitoring dashboards updated
- [ ] Alert thresholds reviewed
- [ ] On-call engineer notified

## During Deployment
- [ ] Deployment window communicated
- [ ] Database backups verified
- [ ] Blue-green deployment initiated
- [ ] Health checks passing on new instances
- [ ] Error rates monitored
- [ ] Latency monitored
- [ ] Database connection pool stable

## Post-Deployment
- [ ] Smoke tests passed
- [ ] All health checks green
- [ ] Error rates within threshold
- [ ] Response times within SLA
- [ ] No database deadlocks detected
- [ ] Cache hit rates normal
- [ ] Queue depths normal
- [ ] Deployment documented in runbook
- [ ] Stakeholders notified

## Rollback Procedure
If any post-deployment check fails:
1. Execute: `./scripts/deployment/rollback.sh production`
2. Verify rollback health checks
3. Investigate root cause
4. Document incident in post-mortem
