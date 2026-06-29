# TaskCo

A task management web app built with ASP.NET Core 9, Razor Pages, and Bootstrap 5.

## Features

- **Authentication** — register and log in with email/password (ASP.NET Core Identity, cookie auth)
- **Projects** — create, edit, and delete projects with an optional due date
- **Tasks** — create and manage tasks within projects; track status (Todo / In Progress / Done) and priority (Low / Medium / High)
- **Dashboard** — overview of total, in-progress, completed, and overdue tasks; per-project cards with progress bars

## Tech stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 9 (Razor Pages + Web API) |
| ORM | EF Core 9 with SQL Server Express (dev) / InMemory (tests) |
| Auth | ASP.NET Core Identity |
| Validation | FluentValidation 11 |
| UI | Bootstrap 5.3 (CDN) |
| Tests | xUnit + `Microsoft.AspNetCore.Mvc.Testing` |

## Getting started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server Express — instance `.\SQLEXPRESS`, database `TaskCoDB` must exist

### Run

```powershell
# Apply database migrations (first time only)
cd src\TaskManager.Web
dotnet ef database update

# Build and start the app
dotnet build -c Release
dotnet bin\Release\net9.0\TaskManager.Web.dll
# → http://localhost:5000

# Optional: run on a different port
dotnet bin\Release\net9.0\TaskManager.Web.dll --urls http://localhost:5196
```

### Run tests

```powershell
# Must use Release on Windows (AppControl policy)
dotnet test tests\TaskManager.Tests -c Release --output %TEMP%\tc-out
```

## Project structure

```
src/
  TaskManager.Web/
    Controllers/        # REST API endpoints
    Pages/              # Razor Pages (UI)
    Services/           # Business logic + DB access
    Models/
      Entities/         # EF Core entity types
      Dtos/             # Request / Response records
    Validators/         # FluentValidation validators
    Data/               # DbContext + migrations
    Common/             # Result<T>, Error, ApiControllerBase
tests/
  TaskManager.Tests/
    Unit/               # In-memory DB, FakeCurrentUser
    Integration/        # CustomWebApplicationFactory, TestAuthHandler
```

## API response envelope

```json
// Success
{ "data": <payload> }

// Error
{ "error": { "code": "...", "message": "...", "details": [...] } }
```

Status codes: `200` read/update · `201` create · `400` validation · `401` unauthenticated · `404` not-owned-or-missing
