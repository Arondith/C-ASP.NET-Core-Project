# TaskForge API

A production-style project and task management REST API built with **C# and ASP.NET Core**.

TaskForge is designed as a portfolio backend that demonstrates more than CRUD: secure authentication, authorization, relational data modeling, business rules, filtering, pagination, API documentation, rate limiting, health checks, automated tests, Docker, and CI.

## Tech stack

- C# / .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- JWT Bearer authentication
- ASP.NET Core Identity password hashing
- OpenAPI
- xUnit
- Docker
- GitHub Actions

## Features

- Register and sign in with JWT authentication
- Passwords hashed with ASP.NET Core's `PasswordHasher<TUser>`
- Project ownership and role-based authorization
- Create, read, update, and delete projects
- Nested work-item endpoints per project
- Work-item status and priority filtering
- Pagination
- Domain rules for valid task-status transitions
- Project statistics endpoint
- Problem Details error responses
- Built-in rate limiting
- Health endpoint
- SQLite persistence
- Automated tests and CI build
- Docker-ready setup

## Project structure

```text
src/TaskForge.Api/
  Controllers/   HTTP endpoints
  Data/          EF Core DbContext
  Domain/        Entities and enums
  Dtos/          API request/response contracts
  Services/      Token generation and domain rules

tests/TaskForge.Api.Tests/
  WorkItemRulesTests.cs

.github/workflows/
  dotnet.yml
```

## Run locally

### Prerequisites

- .NET 10 SDK

### Start the API

```bash
dotnet restore
dotnet run --project src/TaskForge.Api
```

The application creates a local SQLite database on first run.

OpenAPI JSON is available at:

```text
/openapi/v1.json
```

Health check:

```text
/health
```

## Authentication flow

1. `POST /api/auth/register`
2. `POST /api/auth/login`
3. Copy the returned token.
4. Send it as `Authorization: Bearer <token>` to protected endpoints.

Example registration payload:

```json
{
  "email": "developer@example.com",
  "password": "StrongPass123!"
}
```

## Example endpoints

| Method | Endpoint | Purpose |
| --- | --- | --- |
| POST | `/api/auth/register` | Create an account |
| POST | `/api/auth/login` | Sign in |
| GET | `/api/projects` | List your projects |
| POST | `/api/projects` | Create a project |
| GET | `/api/projects/{id}` | Get project details |
| GET | `/api/projects/{id}/stats` | Get project statistics |
| GET | `/api/projects/{projectId}/work-items` | Filter/paginate work items |
| POST | `/api/projects/{projectId}/work-items` | Create a work item |
| PUT | `/api/projects/{projectId}/work-items/{id}` | Update a work item |
| DELETE | `/api/projects/{projectId}/work-items/{id}` | Delete a work item |

## Why this project is portfolio-ready

This repository demonstrates backend concepts commonly expected from junior .NET developers: REST API design, EF Core relationships, DTO separation, authentication, authorization, domain validation, async database access, dependency injection, configuration, automated tests, CI, and containerization.

## Security note

The JWT key in `appsettings.json` is only a development placeholder. For deployment, provide `Jwt__Key` through environment variables or a secret manager.

## License

MIT
