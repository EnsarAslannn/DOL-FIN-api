# 🐬 DOL-FIN API

[![CI](https://github.com/EnsarAslannn/DOL-FIN-api/actions/workflows/ci.yml/badge.svg)](https://github.com/EnsarAslannn/DOL-FIN-api/actions/workflows/ci.yml)

The backend for [DOL-FIN](https://github.com/EnsarAslannn/DOL-FIN), an enterprise-focused financial management platform: an ASP.NET Core Web API handling authentication, portfolio management, and stock/comment data over a relational PostgreSQL schema.

**Live API:** [api-production-a1b64.up.railway.app](https://api-production-a1b64.up.railway.app) — used by the live app at [dol-fin.com](https://dol-fin.com)

The frontend (React/TypeScript) lives in a separate repo: [DOL-FIN](https://github.com/EnsarAslannn/DOL-FIN).

## Features

- **Authentication:** ASP.NET Core Identity, JWT delivered via an httpOnly cookie (not exposed to JS), role-based authorization (`Admin`/`User`).
- **CSRF Protection:** Double-submit cookie pattern (`IAntiforgery`) on every authenticated, state-changing request.
- **Portfolio Engine:** Buy/sell/deposit/withdraw with transactional consistency (`IUnitOfWork`), ownership-checked comment CRUD, admin-only stock management.
- **Rate Limiting:** IP-partitioned fixed-window limiter on login/register.
- **API Docs:** Interactive OpenAPI docs via [Scalar](https://github.com/scalar/scalar) at `/scalar` (development environment).

## Tech Stack

.NET 10.0 & ASP.NET Core Web API, Entity Framework Core & PostgreSQL (Npgsql), ASP.NET Core Identity & JWT Bearer auth, Serilog, xUnit + Moq for testing.

Deployed on [Railway](https://railway.app) via Docker; database migrations run automatically on startup.

## Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A local PostgreSQL instance (or a connection string to any reachable one)

### Steps

```bash
git clone https://github.com/EnsarAslannn/DOL-FIN-api.git
cd DOL-FIN-api
dotnet restore
```

Set secrets locally via `dotnet user-secrets` — these must never be committed to `appsettings.json`:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=dolfin;Username=postgres;Password=..."
dotnet user-secrets set "JWT:SigningKey" "<a random key at least 64 characters long>"
```

Apply migrations and run:

```bash
dotnet ef database update
dotnet run
```

The API listens on `https://localhost:7109` by default; interactive docs are at `/scalar`.

To make a locally-registered user an admin, set `Admin:SeedUsername` (via `dotnet user-secrets` or `appsettings.Development.json`) to that user's username and restart — the app grants the `Admin` role to a matching user on startup if they aren't already in it.

## Tests

```bash
dotnet test
```

14 tests covering comment ownership authorization (IDOR regression), stock CRUD role restrictions, and JWT role-claim generation.

## Project Structure

```
Controllers/    API endpoints (Account, Stock, Portfolio, Comment)
Service/        Business logic (PortfolioService, TokenService)
Repository/     Data access behind interfaces
Dtos/           Request/response contracts
Models/         EF Core entities
Migrations/     EF Core migrations
api.Tests/      xUnit test project
```

## License

[MIT](./LICENSE)

## Contact

**Ensar Aslan** — [GitHub](https://github.com/EnsarAslannn)
