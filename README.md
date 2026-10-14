# 🚀 Ecommerce CQRS API (.NET 9)

Um backend de e-commerce moderno, robusto e escalável de alta performance, construído com **.NET 9**, seguindo os princípios de **Clean Architecture**, **CQRS**, **Persistência Poliglota** (SQL Server + MongoDB), **Mensageria Assíncrona** (RabbitMQ + MassTransit), **Cache Distribuído** (Redis) e **Autenticação JWT com Policies**.

---

## 🛠️ Tech Stack & Ferramentas

* **Linguagem & Framework:** C# 13, .NET 9, ASP.NET Core Web API
* **Arquitetura:** Clean Architecture, CQRS (MediatR), Domain-Driven Design (DDD)
* **Write Model (Banco Relacional):** SQL Server via Entity Framework Core 9 (Code-First & Migrations)
* **Read Model (Banco Não-Relacional):** MongoDB (Sincronizado via Mensageria)
* **Mensageria & Event-Driven:** MassTransit com RabbitMQ
* **Cache & Invalidation:** Redis (StackExchangeRedis) com Cache-Aside Pattern
* **Validação & Documentação:** FluentValidation, Scalar / OpenAPI (.NET 9 nativo)
* **Segurança:** ASP.NET Core Identity / JWT Bearer com Policy-Based Authorization (`CatalogManagement`, `AdminOnly`)

---


# 🐳 Infraestrutura (Docker)

O projeto depende de serviços externos para mensageria, cache e bancos de dados. Para subir tudo rapidamente, utilize o Docker.

### 1. Baixar e Rodar os Containers
Certifique-se de ter o Docker Desktop instalado e execute os comandos abaixo para puxar as imagens oficiais e iniciar os serviços:

## SQL Server (Write Database)
```docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_Password123" -p 1433:1433 --name sqlserver -d [mcr.microsoft.com/mssql/server:2022-latest](https://mcr.microsoft.com/mssql/server:2022-latest)```

## MongoDB (Read Model Database)
```docker run -p 27017:27017 --name mongodb -d mongo:latest```

## Redis (Distributed Cache)
```docker run -p 6379:6379 --name redis -d redis:latest```

## RabbitMQ (Message Broker com Management UI)
```docker run -p 5672:5672 -p 15672:15672 --name rabbitmq -d rabbitmq:3-management```


# ⚙️ Configuração do Ambiente

1. Clone o repositório na sua máquina
    git clone [https://github.com/AtonementAndy/Ecommerce-CQRS.git](https://github.com/AtonementAndy/Ecommerce-CQRS.git)

   Depois, dentro da pasta do projeto:
    ```cd Ecommerce-CQRS```


Configure as strings de conexão e chaves JWT no arquivo appsettings.json, localizado no projeto Ecommerce.API:

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

# 🏃‍♂️ Como Rodar o Projeto

1. Aplicar as Migrations do EF Core (SQL Server):
Abra o terminal na raiz do projeto e execute:
```
dotnet ef database update --project src/Ecommerce.Infrastructure --startup-project src/Ecommerce.API
```

2. Executar a API:
```
dotnet run --project src/Ecommerce.API
```

Acessar a Documentação (Scalar / OpenAPI):
Com a aplicação rodando, abra o navegador e acesse a rota do Scalar configurada para testar os endpoints de catálogo, autenticação e gerenciamento de produtos.
Obs.: Assim que o projeto rodar, pegue a porta do localhost e mude aqui: 
```
https://localhost:7238/scalar/v1
```

Autenticação e Perfis (Roles / Policies)
Os endpoints de alteração e gerenciamento exigem tokens JWT válidos gerados com as claims correspondentes (CatalogManagement ou Admin para exclusões).

# 🔑 Como Gerar um Token JWT para Testes

Como a API valida tokens JWT assinados com a chave secreta configurada no seu appsettings.json, você pode gerar um token de teste rapidamente através do site jwt.io:
1. Acesse o site: [https://jwt.io/](https://jwt.io/)
2. Configure o Header (Cabeçalho):
```
{
  "alg": "HS256",
  "typ": "JWT"
}
```
3. Configure o Payload (Corpo com as permissões/Roles):
   Para testar rotas de gerenciamento (CatalogManagement):
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
Para testar rotas restritas de exclusão (AdminOnly):
Mude o campo "role" para "Admin".

4. Assine o Token:
  Na parte inferior da caixa de assinatura (Verify Signature), cole a mesma chave secreta que está configurada no seu JwtSettings:Secret no arquivo appsettings.json.

5. Copie e Use:
  Copie o token gerado na caixa da esquerda (Encoded), vá até a interface do Scalar na sua API,
  clique no botão de autorização (Bearer) e cole o token para conseguir disparar as requisições protegidas com sucesso!


