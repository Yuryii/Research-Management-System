# RMS — Resource Management System

A full-stack web application built on **Clean Architecture** principles, using **.NET 10** with **Angular 21**.

## Overview

RMS is a workflow and approval management system supporting configurable multi-step application processes with role-based permissions. It was scaffolded from the [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) and extended with domain entities for application lifecycle management.

## Architecture

The solution follows Clean Architecture / Domain-Driven Design with an **Aspire** orchestrator:

```
RMS/
├── src/
│   ├── Domain/           Entities, Value Objects, Domain Events, Enums
│   ├── Application/      CQRS via MediatR — Commands, Queries, Behaviors
│   ├── Infrastructure/   EF Core, Identity, Interceptors, Configurations
│   ├── Web/              ASP.NET Core Web API + Angular SPA (ClientApp)
│   ├── Shared/           Shared code for Aspire
│   ├── ServiceDefaults/  Aspire service defaults (OpenTelemetry, resilience)
│   └── AppHost/          Aspire App Host (orchestrator)
└── tests/
    ├── Application.UnitTests/
    ├── Domain.UnitTests/
    ├── Infrastructure.IntegrationTests/
    └── Web.AcceptanceTests/
```

### Key Patterns

- **CQRS** via MediatR — Commands and Queries are fully separated
- **Domain Events** — automatically dispatched on save via `DispatchDomainEventsInterceptor`
- **Auditable Entities** — `BaseAuditableEntity` with created/updated timestamps
- **Value Objects** — immutable components (e.g., `Colour`)
- **Pipeline Behaviors** — Performance, Logging, UnhandledException, Authorization, Validation
- **FluentValidation** — declarative input validation
- **AutoMapper** — DTO-to-Entity mapping
- **ASP.NET Identity** — authentication and authorization
- **OpenTelemetry** — distributed tracing via Aspire ServiceDefaults

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10, ASP.NET Core, Entity Framework Core |
| Database | SQL Server (local: `mssqllocaldb`, database: `RMSDb`) |
| Frontend | Angular 21, TypeScript, RxJS |
| Styling | Pico CSS (classless CSS framework) |
| Icons | Lucide Angular |
| API Client | Auto-generated via NSwag from OpenAPI spec |
| Orchestration | .NET Aspire |
| Testing | xUnit, FluentAssertions, NSubstitute, Respawn |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/)
- [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) or `mssqllocaldb`
- (Optional) [.NET Aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling): `dotnet workload install aspire`

### 1. Restore & Build

```bash
cd d:\Projects\CA-RMS\RMS\RMS
dotnet restore src.sln
dotnet build src.sln
```

### 2. Configure Database

Update the SQL Server connection string in `src/Web/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RMSDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Apply migrations and seed the database:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

### 3. Run the Application

**Using Aspire (recommended):**

```bash
dotnet run --project src/AppHost
```

This starts the Aspire dashboard at `http://localhost:9180` alongside the Web API and Angular dev server.

**Without Aspire:**

```bash
dotnet run --project src/Web
```

Navigate to `http://localhost:5000` for the Web API or `http://localhost:5000` (Angular served separately via `ng serve`).

### 4. Run Frontend in Isolation

```bash
cd src/Web/ClientApp
npm install
ng serve
```

Navigate to `http://localhost:4200/`.

### 5. Run Tests

```bash
dotnet test
```

## Project Structure

### Domain Layer (`src/Domain/`)

Contains pure business logic with no external dependencies:

- **Entities** — `Application`, `Step`, `StepDetail`, `RoleStepPermission`, `ApplicationFile`, `File`, `AcademicDegree`, `ResearchHour`
- **Enums** — `Priority`
- **Value Objects** — `Colour`
- **Events** — domain events raised by entities
- **Interfaces** — repository contracts

### Application Layer (`src/Application/`)

Orchestrates domain objects and exposes use cases:

- **Commands** — create/update/delete operations (MediatR `IRequest`)
- **Queries** — read operations (MediatR `IRequest<IReadOnlyList<T>>`)
- **Behaviors** — cross-cutting concerns (logging, validation, performance, authorization)
- **DTOs** — data transfer objects with AutoMapper profiles
- **Interfaces** — service abstractions

### Infrastructure Layer (`src/Infrastructure/`)

Implements Application interfaces:

- **Data** — EF Core `DbContext`, entity `Configurations`, `Interceptors`
- **Identity** — ASP.NET Identity setup
- **Services** — file storage, email, external service integrations

### Web Layer (`src/Web/`)

The API surface and Angular SPA host:

- **API Controllers** — minimal controllers dispatching to MediatR
- **ClientApp/** — Angular 21 single-page application
  - **Core/** — services, guards, interceptors
  - **Features/** — feature modules (auth, dashboard, applications)
  - **Shared/** — shared components, models, pipes

## API Documentation

The OpenAPI specification is auto-generated and available at:

```
src/Web/wwwroot/openapi/v1.json
```

It can be viewed in Swagger UI when running with `Swashbuckle.AspNetCore`.

## Authentication

RMS uses ASP.NET Identity with JWT bearer tokens and cookie-based authentication.

### Available Endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/Users/register` | Register a new user |
| POST | `/api/Users/login` | Login and receive tokens |
| POST | `/api/Users/refresh` | Refresh access token |
| POST | `/api/Users/logout` | Logout and invalidate session |
| GET | `/api/Users/confirmEmail` | Confirm email address |
| POST | `/api/Users/manage/2fa` | Manage two-factor authentication |
| GET | `/api/Users/manage/info` | Get account information |

## License

This project is licensed under the MIT License. See [LICENSE.txt](LICENSE.txt) for details.

## Acknowledgements

- [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) by Jason Taylor
- [Angular](https://angular.io/)
- [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Pico CSS](https://picocss.com/)
- [Lucide Icons](https://lucide.dev/)
