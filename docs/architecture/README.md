# Architecture Decision Record (ADR)

## Project: E-Commerce Platform (SPEC-001)

### ADR-001: Modular Monolith Architecture
- **Status:** Accepted
- **Date:** 2026-05-27
- **Decision:** Start with a modular monolith using Clean Architecture before extracting microservices.
- **Rationale:** Reduces premature complexity while enforcing strict module boundaries that enable future extraction.
- **Consequences:** All modules share the same deployment unit. Must maintain strict dependency rules.

### ADR-002: ASP.NET Core 8 with Clean Architecture
- **Status:** Accepted
- **Decision:** Use ASP.NET Core 8 with Clean Architecture layers (Api, Application, Domain, Infrastructure, Shared).
- **Rationale:** Industry standard for enterprise .NET applications. Clear separation of concerns.
- **Consequences:** Requires discipline to maintain layer boundaries. Dependency injection is mandatory.

### ADR-003: PostgreSQL with Entity Framework Core 8
- **Status:** Accepted
- **Decision:** PostgreSQL 16 with EF Core 8 for data persistence using Npgsql provider.
- **Rationale:** Open-source, feature-rich relational database with strong .NET support.
- **Consequences:** Must use EF Core migrations for schema changes. SSL disabled for local development.

### ADR-004: Docker Compose for Local Development
- **Status:** Accepted
- **Decision:** Use Docker Compose for local development services (PostgreSQL, Redis, RabbitMQ).
- **Rationale:** Ensures consistent development environments. Ephemeral by default.
- **Consequences:** Docker Desktop required. Services reset on `down -v`.

### ADR-005: CQRS with MediatR
- **Status:** Accepted
- **Decision:** Implement CQRS pattern using MediatR for command/query separation.
- **Rationale:** Cleaner separation of read/write operations. Enables independent scaling.
- **Consequences:** Additional abstraction. Each feature requires command/query classes.

### ADR-006: Repository Pattern with Unit of Work
- **Status:** Accepted
- **Decision:** Use repository pattern with Unit of Work for data access abstraction.
- **Rationale:** Decouples domain from persistence. Simplifies testing.
- **Consequences:** Additional abstraction layer over DbContext.

## Domain Model

### Aggregate Roots
| Aggregate | Entities              | Purpose                        |
|-----------|-----------------------|--------------------------------|
| Product   | Product, ProductReview| Product catalog with inventory |
| Category  | Category              | Hierarchical product categories|
| Order     | Order, OrderItem      | Customer orders                |
| Cart      | Cart, CartItem        | Shopping cart                  |
| User      | User                  | User accounts with roles       |

### Value Objects
| Value Object      | Properties                          | Usage                  |
|-------------------|-------------------------------------|------------------------|
| Money             | Amount, Currency                    | Currency-aware pricing |
| Sku               | Value                               | Product SKU identifier |
| ProductDimensions | Length, Width, Height, Weight, Unit | Physical measurements  |

### Domain Events
- `ProductCreatedEvent` - New product added to catalog
- `ProductPriceUpdatedEvent` - Price change tracking
- `StockReservedEvent` - Inventory reservation during checkout
- `StockConfirmedEvent` - Reservation confirmed
- `StockReleasedEvent` - Reservation released
- `LowStockEvent` - Alert when stock falls below threshold
- `ProductReviewedEvent` - New product review
- `ProductDeactivatedEvent` - Product removed from active catalog

## Database Schema

### Connection String (Development)

Host=localhost;Port=5432;Database=ecommerce_dev;Username=ecommerce_user;Password=dev_password_change_in_production;SSL Mode=Disable

### Tables Created by Initial Migration
- `products` - Core product table with owned types (price_amount, price_currency, sku, dim_*)
- `categories` - Product categories with self-referencing parent_category_id
- `__EFMigrationsHistory` - Migration tracking

### EF Core Configuration
- `ProductConfiguration` - Fluent API for Product entity
- Owned types: Money (price), Sku, ProductDimensions
- Optimistic concurrency via RowVersion
- Composite indexes for performance

## API Design

### Endpoints
| Method | Path                    | Description            |
|--------|-------------------------|------------------------|
| GET    | /health                 | General health check   |
| GET    | /api/v1/products/health | Product endpoint health|
| POST   | /api/v1/products        | Create product         |
| GET    | /api/v1/products/{id}   | Get product by ID      |

### Middleware Pipeline
1. `CorrelationIdMiddleware` - Request correlation ID
2. `RequestLoggingMiddleware` - Structured request logging
3. `ExceptionHandlingMiddleware` - Centralized error handling

### Features (CQRS)
| Feature        | Command/Query                                | Validator                     |
|----------------|----------------------------------------------|-------------------------------|
| Create Product | CreateProductCommand → CreateProductResponse | CreateProductCommandValidator |
| Get Product    | GetProductByIdQuery → ProductDetailDto       |           -                   |

## Infrastructure Services

| Service       | Implementation        | Interface              |
|---------------|-----------------------|------------------------|
| Cache         | MemoryCacheService    | ICacheService          |
| Repository    | ProductRepository     | IProductRepository     |
| Unit of Work  | UnitOfWork            | IUnitOfWork            |
| Domain Events | DomainEventDispatcher | IDomainEventDispatcher |

## Docker Services

| Service    | Image                        | Port        |
|------------|------------------------------|-------------|
| PostgreSQL | postgres:16-alpine           | 5432        |
| Redis      | redis:7-alpine               | 6379        |
| RabbitMQ   | rabbitmq:3-management-alpine | 5672, 15672 |
