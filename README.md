# 🚀 Ecommerce CQRS API (.NET 9)

A modern, robust, and highly scalable high-performance e-commerce backend, built with .NET 9, following the principles of Clean Architecture, CQRS, Polyglot Persistence (SQL Server + MongoDB), Asynchronous Messaging (RabbitMQ + MassTransit), Distributed Cache (Redis), and JWT Authentication with Policies.


## 🛠️ Tech Stack & Tools

Language and Framework: C# 13, .NET 9, ASP.NET Core Web API
Architecture: Clean Architecture, CQRS (MediatR), Domain-Driven Design (DDD)
Write Model (Relational Database): SQL Server via Entity Framework Core 9 (Code-First and Migrations)
Read Model (Non-Relational Database): MongoDB (Syncing with MessageBroker)
MessageBroker and Event-Driven: MassTransit com RabbitMQ
Cache & Invalidation: Redis (StackExchangeRedis) with Cache-Aside Pattern
Validation and Documentation: FluentValidation, Scalar / OpenAPI (.NET 9 nativo)
Security: ASP.NET Core Identity / JWT Bearer com Policy-Based Authorization (CatalogManagement, AdminOnly)


# 🐳 Infrastructure (Docker)

The project depends on external services for messaging, caching, and databases. To spin everything up quickly, use Docker.

### 1. Download and Run the Containers
Make sure you have Docker Desktop installed and run the commands below to pull the official images and start the services:

## SQL Server (Write Database)
```
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_Password123" -p 1433:1433 --name sqlserver -d [mcr.microsoft.com/mssql/server:2022-latest](https://mcr.microsoft.com/mssql/server:2022-latest)
```

## MongoDB (Read Model Database)
```
docker run -p 27017:27017 --name mongodb -d mongo:latest
```

## Redis (Distributed Cache)
```
docker run -p 6379:6379 --name redis -d redis:latest
```

## RabbitMQ (Message Broker com Management UI)
```
docker run -p 5672:5672 -p 15672:15672 --name rabbitmq -d rabbitmq:3-management
```


# ⚙️ Environment Configuration

Clone the repository to your machine:
git clone [https://github.com/AtonementAndy/Ecommerce-CQRS.git](https://github.com/AtonementAndy/Ecommerce-CQRS.git)

Then, inside the project folder:
```
cd Ecommerce-CQRS
```


Configure the connection strings and JWT keys in the appsettings.json file, located in the Ecommerce.API project:

```"ConnectionStrings"
    "DefaultConnection": "Server=localhost,1433;Database=EcommerceDb;User Id=sa;Password=Your_Password123;TrustServerCertificate=True;Encrypt=Optional;",
    "MongoDb": "mongodb://localhost:27017",
    "Redis": "localhost:6379"

  "JwtSettings": 
    "Secret": "SUA_CHAVE_SECRETA_SUPER_SEGURA_E_LONGA_AQUI",
    "Issuer": "Ecommerce.API",
    "Audience": "shop-api"

"MongoDbSettings": {
  "DatabaseName": "EcommerceDb",
  "CollectionName": "ProductsReadModel"
```

# 🏃‍♂️ How to run the Project

1. Apply the EF Core Migrations (SQL Server):
Open a terminal at the root of the project and run:
```
dotnet ef database update --project src/Ecommerce.Infrastructure --startup-project src/Ecommerce.API
```

2. Execute the API:
```
dotnet run --project src/Ecommerce.API
```

Access the Documentation (Scalar / OpenAPI):
With the application running, open your browser and go to the Scalar route configured to test the catalog, authentication, and product management endpoints.
Note: As soon as the project runs, grab the localhost port and change it here:
```
https://localhost:7238/scalar/v1
```

Authentication and Profiles (Roles / Policies)
The modification and management endpoints require valid JWT tokens generated with the corresponding claims (CatalogManagement or Admin for deletions).

# 🔑 How to Generate a JWT Token for Testing

Since the API validates JWT tokens signed with the secret key configured in your appsettings.json, you can quickly generate a test token through the jwt.io website:
1. Go to the website: [https://jwt.io/](https://jwt.io/)
2. Configure the Header:
```
{
  "alg": "HS256",
  "typ": "JWT"
}
```
3. Configure the Payload (Body with the permissions/Roles):
   To test management routes (CatalogManagement):
```
{
  "sub": "1234567890",
  "name": "Admin User",
  "role": "Manager",
  "permission": "catalog:write",
  "iss": "Ecommerce.API",
  "aud": "shop-api",
  "exp": 2000000000
}
```
To test restricted deletion routes (AdminOnly):
Change the "role" field to "Admin".

4. Sign the Token:
   At the bottom of the signature box (Verify Signature), paste the same secret key that is configured in your JwtSettings:Secret in the appsettings.json file.

6. CCopy and Use:
    Copy the token generated in the left box (Encoded), go to the Scalar interface in your API,
    click the authorization button (Bearer), and paste the token so you can successfully fire the protected requests!


