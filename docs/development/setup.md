# Development Setup Guide

## Prerequisites

- .NET 8 SDK
- Docker Desktop
- PowerShell or Bash terminal
- VS Code or Visual Studio 2022

## Quick Start

### 1. Clone and Bootstrap

```bash
git clone <repository-url>
cd ecommerce-platform
```

### 2. Start Docker Services

```bash
docker compose up -d
```

Services started:
- PostgreSQL 16 (port 5432)
- Redis 7 (port 6379)
- RabbitMQ 3 (ports 5672, 15672)

### 3. Restore and Build

```bash
dotnet restore
dotnet build --configuration Release
```
Expected: 5 projects built, 0 errors, 0 warnings.

### 4. Apply Database Migrations

```bash
cd apps/api/ECommerce.Infrastructure
dotnet ef database update --startup-project ../ECommerce.Api/ECommerce.Api.csproj
```

### 5. Run the API

```bash
cd apps/api/ECommerce.Api
dotnet run
```

API available at: http://localhost:5000
Swagger UI: http://localhost:5000/swagger

## Verification

```bash
# Health check
curl http://localhost:5000/health
```

```bash
# Product endpoint health
curl http://localhost:5000/api/v1/products/health
```

## Project Structure

```bash
ecommerce-platform/
├── ecommerce-platform.sln                          # Solution file
├── docker-compose.yml                              # Development services
├── README.md                                       # Project overview
├── .gitignore                                      # Git exclusions
│
├── apps/
│   ├── api/
│   │   ├── Dockerfile                              # Production API image
│   │   ├── ECommerce.Api/                          # ASP.NET Core Web API
│   │   │   ├── Program.cs                          # Application entry point
│   │   │   ├── appsettings.json                    # Configuration
│   │   │   ├── GlobalUsings.cs                     # Global namespace imports
│   │   │   ├── Controllers/
│   │   │   │   └── ProductsController.cs
│   │   │   └── Middleware/
│   │   │       ├── CorrelationIdMiddleware.cs
│   │   │       ├── ExceptionHandlingMiddleware.cs
│   │   │       └── RequestLoggingMiddleware.cs
│   │   │
│   │   ├── ECommerce.Domain/                       # Domain layer (entities, value objects)
│   │   │   ├── Base/
│   │   │   │   ├── AggregateRoot.cs                # Base class for aggregates
│   │   │   │   └── ValueObject.cs                  # Base class for value objects
│   │   │   ├── Entities/
│   │   │   │   ├── Product.cs                      # Product aggregate
│   │   │   │   ├── Category.cs                     # Category entity
│   │   │   │   ├── Order.cs                        # Order aggregate + OrderItem
│   │   │   │   ├── Cart.cs                         # Cart aggregate + CartItem
│   │   │   │   ├── Coupon.cs                       # Coupon entity
│   │   │   │   └── User.cs                         # User entity
│   │   │   ├── ValueObjects/
│   │   │   │   ├── Money.cs                        # Currency-aware monetary values
│   │   │   │   ├── Sku.cs                          # Stock Keeping Unit
│   │   │   │   └── ProductDimensions.cs            # Physical dimensions
│   │   │   ├── Enums/
│   │   │   │   └── OrderStatus.cs                  # OrderStatus, PaymentStatus
│   │   │   ├── Exceptions/
│   │   │   │   └── DomainException.cs              # Custom domain exceptions
│   │   │   └── Interfaces/
│   │   │       ├── IProductRepository.cs
│   │   │       ├── ICategoryRepository.cs
│   │   │       ├── IOrderRepository.cs
│   │   │       ├── IUnitOfWork.cs
│   │   │       ├── IDomainEventDispatcher.cs
│   │   │       └── ICacheService.cs
│   │   │
│   │   ├── ECommerce.Application/                  # Application layer (CQRS handlers)
│   │   │   ├── DependencyInjection.cs              # Service registration
│   │   │   ├── GlobalUsings.cs
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs           # MediatR validation pipeline
│   │   │   │   └── LoggingBehavior.cs              # MediatR logging pipeline
│   │   │   └── Features/
│   │   │       └── Products/
│   │   │           ├── Commands/
│   │   │           │   └── CreateProduct/
│   │   │           │       └── CreateProductCommand.cs
│   │   │           └── Queries/
│   │   │               └── GetProductById/
│   │   │                   └── GetProductByIdQuery.cs
│   │   │
│   │   ├── ECommerce.Infrastructure/               # Infrastructure layer
│   │   │   ├── DependencyInjection.cs              # Service registration
│   │   │   ├── GlobalUsings.cs
│   │   │   ├── Persistence/
│   │   │   │   ├── ECommerceDbContext.cs           # EF Core DbContext
│   │   │   │   ├── ECommerceDbContextFactory.cs    # Design-time factory
│   │   │   │   ├── UnitOfWork.cs                   # Transaction management
│   │   │   │   ├── DomainEventDispatcher.cs        # Domain event dispatch
│   │   │   │   ├── Configurations/
│   │   │   │   │   └── ProductConfiguration.cs     # EF Fluent API config
│   │   │   │   └── Repositories/
│   │   │   │       └── ProductRepository.cs
│   │   │   ├── Caching/
│   │   │   │   └── MemoryCacheService.cs           # In-memory cache
│   │   │   ├── Migrations/                         # EF Core migrations
│   │   │   │   ├── 20260527004648_InitialCreate.cs
│   │   │   │   ├── 20260527004648_InitialCreate.Designer.cs
│   │   │   │   └── ECommerceDbContextModelSnapshot.cs
│   │   │   └── Monitoring/
│   │   │       └── ApplicationMetrics.cs           # Custom metrics
│   │   │
│   │   └── ECommerce.Shared/                       # Shared utilities
│   │       └── ECommerce.Shared.csproj
│   │
│   ├── frontend/                                   # React frontend (placeholder)
│   │   ├── Dockerfile
│   │   ├── nginx.conf
│   │   └── src/
│   │
│   └── worker/                                     # Background worker (placeholder)
│       └── src/
│
├── packages/
│   ├── shared-types/                               # Shared TypeScript types
│   └── ui/                                         # Shared UI components
│
├── infra/
│   ├── cdk/                                        # AWS CDK infrastructure
│   │   ├── bin/
│   │   │   └── ecommerce-infra.ts
│   │   └── lib/
│   │       └── ecommerce-stack.ts
│   ├── docker/                                     # Additional Docker files
│   └── terraform/                                  # Terraform configs (placeholder)
│       ├── environments/
│       └── modules/
│
├── scripts/
│   ├── db/
│   │   └── init.sql                                # Database initialization
│   ├── deployment/
│   │   └── deploy.sh                               # Deployment script
│   ├── env/
│   │   └── setup-env.sh                            # Environment configuration
│   └── monitoring/
│       └── setup-alerts.sh                         # CloudWatch alerts setup
│
├── tests/
│   ├── unit/                                       # Unit tests (placeholder)
│   └── integration/                                # Integration tests (placeholder)
│
└── docs/
    ├── architecture/
    │   ├── README.md                               # Architecture decisions
    │   └── security.md                             # Security model
    └── operations/
        ├── deployment-checklist.md                 # Production deployment checklist
        └── deployment-flow.md                      # Deployment pipeline flow
```        

## Layer Dependencies

```bash
ECommerce.Api
    ├──→ ECommerce.Application
    └──→ ECommerce.Infrastructure

ECommerce.Application
    ├──→ ECommerce.Domain
    └──→ ECommerce.Shared

ECommerce.Infrastructure
    ├──→ ECommerce.Application
    └──→ ECommerce.Domain

ECommerce.Domain
    └──→ (no internal dependencies)

ECommerce.Shared
    └──→ (no internal dependencies)
```

## Key Commands

### Build

```bash
dotnet build --configuration Release
```

### Test

```bash
dotnet test
```

### Add EF Core Migration

```bash
cd apps/api/ECommerce.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../ECommerce.Api/ECommerce.Api.csproj
dotnet ef database update --startup-project ../ECommerce.Api/ECommerce.Api.csproj
```

### Docker

```bash
# Start all services
docker compose up -d

# Stop all services
docker compose down

# Reset database
docker compose down -v
docker compose up -d
```

## Troubleshooting

### "Failed to restore NuGet packages"

```bash
dotnet nuget locals all --clear
dotnet restore
```

### "no pg_hba.conf entry" or "EndOfStreamException"

```bash
docker compose down -v
docker compose up -d
Start-Sleep -Seconds 10
cd apps/api/ECommerce.Infrastructure
dotnet ef database update --startup-project ../ECommerce.Api/ECommerce.Api.csproj
```

### Port 5432 already in use

```bash
# Find the process
netstat -ano | findstr :5432
# Stop conflicting PostgreSQL service or change port in docker-compose.yml
```

### EF Core tool not found

```bash
dotnet tool install --global dotnet-ef --version 8.0.0
# Use dotnet-ef (with hyphen) instead of dotnet ef
dotnet-ef --version
```
