# Operations Guide

## Environments

| Environment | Purpose                | Database          |
|-------------|------------------------|-------------------|
| Development | Local development      | Docker PostgreSQL |
| Staging     | Pre-production testing | AWS RDS           |
| Production  | Live environment       | AWS RDS Multi-AZ  |

## Docker Services

### Start All Services

```bash
docker compose up -d
```

### Stop All Services

```bash
docker compose down
```

### View Service Logs

```bash
docker compose logs -f postgres
docker compose logs -f redis
docker compose logs -f rabbitmq
```

### Reset Database (Destroys All Data)

```bash
docker compose down -v
docker compose up -d
cd apps/api/ECommerce.Infrastructure
dotnet ef database update --startup-project ../ECommerce.Api/ECommerce.Api.csproj
```

### Access Services Directly

```bash
# PostgreSQL
docker exec -it ecommerce-postgres psql -U ecommerce_user -d ecommerce_dev

# Redis
docker exec -it ecommerce-redis redis-cli

# RabbitMQ Management UI
# Browser: http://localhost:15672
# Credentials: ecommerce_user / dev_password_change_in_production
```

### Health Checks

| Endpoint                   | Expected Response                     |
|----------------------------|---------------------------------------|
| GET /health                | Healthy                               |
| GET /api/v1/products/health| {"status":"Healthy","timestamp":"..."}|

## Database Migrations

### Create Migration

```bash
cd apps/api/ECommerce.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../ECommerce.Api/ECommerce.Api.csproj
```

### Apply Migration

```bash
dotnet ef database update --startup-project ../ECommerce.Api/ECommerce.Api.csproj
```

### Rollback Migration

```bash
dotnet ef database update PreviousMigrationName --startup-project ../ECommerce.Api/ECommerce.Api.csproj
```

### Remove Last Migration (Not Applied)

```bash
dotnet ef migrations remove --startup-project ../ECommerce.Api/ECommerce.Api.csproj
```

## Build & Deploy

### Local Build

```bash
dotnet build --configuration Release
```

### Run Tests

```bash
dotnet test
```

### CI/CD Pipeline

See .github/workflows/ci.yml for the CI pipeline configuration.

## Monitoring

### Application Metrics

- API latency (via RequestLoggingMiddleware)
- Error rates (via ExceptionHandlingMiddleware)
- Database query performance (EF Core logging)

### Docker Health Checks

```bash
docker ps --filter "health=healthy"
docker inspect --format='{{.State.Health.Status}}' ecommerce-postgres
```

## Troubleshooting

### PostgreSQL Connection Issues

```bash
# Check container health
docker ps --filter name=ecommerce-postgres

# Check logs
docker logs ecommerce-postgres --tail 50

# Test connection
docker exec ecommerce-postgres psql -U ecommerce_user -d ecommerce_dev -c "SELECT 1;"
```

### EF Core Migration Issues

```bash
# Remove and recreate migration
dotnet ef migrations remove --startup-project ../ECommerce.Api/ECommerce.Api.csproj
dotnet ef migrations add InitialCreate --startup-project ../ECommerce.Api/ECommerce.Api.csproj
dotnet ef database update --startup-project ../ECommerce.Api/ECommerce.Api.csproj
```
