# E-Commerce Platform (SPEC-001)

Enterprise-grade e-commerce platform built for learning production engineering patterns.

## 🚀 Quick Start

```bash
# Clone
git clone <repository-url>
cd ecommerce-platform

# Start services
docker compose up -d

# Restore and build
dotnet restore
dotnet build --configuration Release

# Apply database migrations
cd apps/api/ECommerce.Infrastructure
dotnet ef database update --startup-project ../ECommerce.Api/ECommerce.Api.csproj

# Run the API
cd ../ECommerce.Api
dotnet run
```

## ✅ Current Status

| Component         | Status                    |
|-------------------|---------------------------|
| Solution Build    | ✅ 5/5 projects, 0 errors |
| PostgreSQL        | ✅ Running (Docker)       |
| Redis             | ✅ Running (Docker)       |
| RabbitMQ          | ✅ Running (Docker)       |
| EF Core Migration | ✅ Applied                |
| API               | ✅ Running on :5000       |
| Health Checks     | ✅ Passing                |

## 🏗 Architecture

```bash
Clean Architecture: Api → Application → Domain
                     Api → Infrastructure → Application
```

- Frontend: React 18 + TypeScript + Vite (planned)
- Backend: ASP.NET Core 8 (Modular Monolith)
- Database: PostgreSQL 16 via Entity Framework Core 8
- Cache: Redis 7
- Queue: RabbitMQ 3
- Patterns: CQRS, Repository, Unit of Work, Domain Events
- Cloud: AWS (ECS, RDS, ElastiCache via CDK)
- CI/CD: GitHub Actions

## 📁 Project Structure

apps/api/
├── ECommerce.Api/             # Web API & Controllers
├── ECommerce.Domain/          # Entities, Value Objects, Interfaces
├── ECommerce.Application/     # CQRS Handlers & Validators
├── ECommerce.Infrastructure/  # EF Core, Repositories, Cache
└── ECommerce.Shared/          # Shared Utilities

## 📋 Features Implemented

### Domain

- Product aggregate with inventory management
- Category with hierarchy support
- Order with items and payment status
- Shopping cart with items
- User with roles
- Coupon with validation logic
- Money value object (currency-aware)
- Domain events for eventual consistency

### API

- Products controller (create, get by id)
- Health check endpoints
- Correlation ID middleware (request tracing)
- Exception handling middleware
- Request logging middleware
- Swagger/OpenAPI documentation

### Infrastructure

- Entity Framework Core with PostgreSQL
- Database migrations
- Product repository (full CRUD + search)
- Unit of Work pattern
- In-memory cache service
- Domain event dispatcher

### DevOps

- Docker Compose (PostgreSQL, Redis, RabbitMQ)
- GitHub Actions CI pipeline
- AWS CDK infrastructure
- Deployment scripts

## 📚 Documentation

- [Architecture Decisions](docs/architecture/README.md)
- [Development Setup](docs/development/setup.md)
- [Operations Guide](docs/operations/README.md)
- [Security Model](docs/architecture/security.md)
- [Deployment Flow](docs/operations/deployment-flow.md)
- [Deployment Checklist](docs/operations/deployment-checklist.md)

## 🔧 Tech Stack

### Backend

- [ASP.NET](https://asp.net) Core 8
- Entity Framework Core 8
- MediatR (CQRS)
- FluentValidation
- Serilog (coming soon)
- Npgsql (PostgreSQL provider)

### Infrastructure

- Docker & Docker Compose
- GitHub Actions
- AWS CDK
- Terraform (planned)

### Planned

- React + TypeScript frontend
- Redis caching
- RabbitMQ event bus
- Elasticsearch search
- Prometheus + Grafana monitoring
