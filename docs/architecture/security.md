
```markdown
# File: docs/architecture/security.md

# Security Model

## Authentication
- JWT-based authentication
- Access tokens: 15-minute expiry
- Refresh tokens: 7-day expiry
- Token rotation on refresh

## Authorization
- Role-based access control (RBAC)
- Roles: Admin, Manager, Customer
- Policy-based authorization for fine-grained control

## Data Protection
- Encryption at rest: RDS encryption, S3 server-side encryption
- Encryption in transit: TLS 1.3
- Secrets management: AWS Secrets Manager
- Parameter encryption: AWS KMS

## API Security
- Rate limiting: 100 requests/minute per IP
- CORS whitelist: Only allowed origins
- Input validation: FluentValidation
- SQL injection protection: Parameterized queries (EF Core)
- CSRF protection for admin endpoints

## Infrastructure Security
- VPC with private subnets for databases
- Security groups with least privilege
- IAM roles with minimal permissions
- WAF rules for common attack patterns
- CloudTrail enabled for audit logging

## Compliance
- PII data encryption
- Audit logging for all data changes
- Data retention policies
- GDPR-compliant data export/deletion
