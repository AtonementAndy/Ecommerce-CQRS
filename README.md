# 🚀 Ecommerce CQRS

Obs.: Eu vou trazer testes, transformar esse projeto em um real Ecommerce adicionando Cart, Order, Payment e mais.

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

1. Clone o repositório na sua máquina:
```
git clone [https://github.com/AtonementAndy/Ecommerce-CQRS.git](https://github.com/AtonementAndy/Ecommerce-CQRS.git)
cd Ecommerce-CQRS
```


Configure as strings de conexão no `appsettings.json` do projeto `Ecommerce.API`. A chave de assinatura JWT não deve ser salva no repositório; configure-a localmente com User Secrets:

```
dotnet user-secrets init --project src/Ecommerce.API
dotnet user-secrets set "Jwt:Key" "<chave aleatória com pelo menos 32 bytes>" --project src/Ecommerce.API
```

Em outros ambientes, forneça o segredo por um gerenciador de segredos ou pela variável de ambiente `Jwt__Key`. A aplicação não inicia se a chave estiver ausente ou tiver menos de 32 bytes. Nunca reutilize uma chave que já tenha sido commitada ou compartilhada.

```"ConnectionStrings"
    "DefaultConnection": "Server=localhost,1433;Database=EcommerceDb;User Id=sa;Password=Your_Password123;TrustServerCertificate=True;Encrypt=Optional;",
    "MongoDb": "mongodb://localhost:27017",
    "Redis": "localhost:6379"

"MongoDbSettings": {
  "DatabaseName": "EcommerceDb",
  "CollectionName": "ProductsReadModel"
```

# 🏃‍♂️ Como Rodar o Projeto

## Testes

Execute todos os testes unitários e de integração a partir da raiz:

```
dotnet test Ecommerce.slnx
```

Os testes de integração da API usam serviços isolados em memória; não precisam iniciar SQL Server, MongoDB, Redis ou RabbitMQ. A chave JWT usada nos testes é exclusiva e não deve ser reutilizada fora deles.

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

# 🔑 Tokens JWT para Testes

<<<<<<< HEAD
Made with ❤️ by [AtonementAndy](https://github.com/AtonementAndy)
=======
A API valida tokens, mas não emite JWTs. Use tokens emitidos pelo provedor de identidade configurado para o ambiente. Para desenvolvimento, se gerar tokens manualmente, faça isso com uma ferramenta local e use somente uma chave de desenvolvimento; não cole uma chave real em sites de terceiros. O token precisa corresponder ao issuer e audience configurados e incluir as roles e permissões exigidas pelas policies (`Manager` ou `Admin` com `catalog:write` para gerenciamento; `Admin` para exclusão).
>>>>>>> 12f36b1 (adding tests)
