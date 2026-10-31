# 🚀 Ecommerce CQRS

A modern, robust and high-performance e-commerce backend built with **.NET 9**, following the principles of **Clean Architecture**, **CQRS**, **Polyglot Persistence** (SQL Server + MongoDB), **Asynchronous Messaging** (RabbitMQ + MassTransit), **Distributed Cache** (Redis) and **JWT Authentication with Policies**.

---

## 🛠️ Tech Stack & Tools

* **Language & Framework:** C# 13, .NET 9, ASP.NET Core Web API
* **Architecture:** Clean Architecture, CQRS (MediatR), Domain-Driven Design (DDD)
* **Write Model (Relational Database):** SQL Server via Entity Framework Core 9 (Code-First & Migrations)
* **Read Model (Non-Relational Database):** MongoDB (synchronized via messaging)
* **Messaging & Event-Driven:** MassTransit with RabbitMQ
* **Cache & Invalidation:** Redis (StackExchangeRedis) with Cache-Aside Pattern
* **Validation & Documentation:** FluentValidation, Scalar / OpenAPI (.NET 9 native)
* **Security:** JWT Bearer with Policy-Based Authorization (`CatalogManagement`, `AdminOnly`)

---

## 🐳 Infrastructure (Docker)

The project depends on external services for messaging, cache and databases. To start everything quickly, use Docker.

### Pull and run the containers

Make sure Docker Desktop is installed and run the following commands:

**SQL Server (Write Database)**

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_Password123" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

## MongoDB (Read Model Database)
```docker run -p 27017:27017 --name mongodb -d mongo:latest```

## Redis (Distributed Cache)
```docker run -p 6379:6379 --name redis -d redis:latest```

## RabbitMQ (Message Broker com Management UI)
```docker run -p 5672:5672 -p 15672:15672 --name rabbitmq -d rabbitmq:3-management```


# ⚙️ Environment Configuration

1. Clone the repository:
```
git clone [https://github.com/AtonementAndy/Ecommerce-CQRS.git](https://github.com/AtonementAndy/Ecommerce-CQRS.git)
cd Ecommerce-CQRS
```


Configure secrets locally using User Secrets (never commit sensitive data): 
```
dotnet user-secrets init --project src/Ecommerce.API
dotnet user-secrets set "Jwt:Key" "<random-key-with-at-least-32-characters>" --project src/Ecommerce.API
dotnet user-secrets set "ConnectionStrings:SqlServer" "Server=localhost,1433;Database=EcommerceDb;User Id=sa;Password=Your_Password123;TrustServerCertificate=True;Encrypt=Optional;" --project src/Ecommerce.API
dotnet user-secrets set "ConnectionStrings:RabbitMQ" "amqp://guest:guest@localhost:5672" --project src/Ecommerce.API
dotnet user-secrets set "ConnectionStrings:MongoDb" "mongodb://root:example@localhost:27017" --project src/Ecommerce.API
```

In other environments, provide the secrets through a secret manager or environment variables (Jwt__Key, ConnectionStrings__SqlServer, etc.). The application will not start if the JWT key is missing or shorter than 32 bytes.

```

# 🏃‍♂️ How to Run the Project

## Tests

Run all unit and integration tests from the root:

```
dotnet test Ecommerce.slnx
```

API integration tests use in-memory services and do not require SQL Server, MongoDB, Redis or RabbitMQ to be running.

1. Apply EF Core migrations:
Apply EF Core migrations:
```
dotnet ef database update --project src/Ecommerce.Infrastructure --startup-project src/Ecommerce.API
```

2. Run the API:
```
dotnet run --project src/Ecommerce.API
```

## API Documentation (Scalar / OpenAPI)
Once the application is running, open the browser and access the Scalar UI (replace the port if needed):
```
https://localhost:7238/scalar/v1
```

Authentication and Policies (Roles / Policies)
Modification and management endpoints require valid JWT tokens with the appropriate claims (CatalogManagement or AdminOnly).

# 🔑 JWT Tokens for Testing
The API validates tokens but does not issue them. Use tokens issued by the configured identity provider.

For local development, generate tokens with a local tool using only a development key. The token must match the configured Issuer and Audience and include the required roles and permissions (Manager or Admin with catalog:write for management; Admin for deletion).


Made with ❤️ by [AtonementAndy](https://github.com/AtonementAndy)

# Architecture Decisions

### Phase 0 – Foundation Cleanup
- Enforced Clean Architecture boundaries (Application no longer depends on EF Core, MongoDB or MassTransit)
- Introduced semantic application exceptions (`NotFoundException`, `ConflictException`)
- Centralized exception-to-HTTP mapping in `GlobalExceptionHandler`
- Extracted JWT authentication setup into `AuthenticationExtensions`
- Moved secrets (JWT key and connection strings) to User Secrets
- Improved separation of concerns and production readiness of the foundation
