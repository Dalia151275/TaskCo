# TaskManager — Claude Code Instructions

## Project layout

```
E:\TaskCo\
  src/TaskManager.Web/      # ASP.NET Core 9 web app (API + Razor Pages)
  tests/TaskManager.Tests/  # xUnit test project
  TaskManager.sln
```

## Stack

- **.NET 9**, EF Core 9, SQLite (dev), EF Core InMemory (tests only)
- **ASP.NET Core Identity** — cookie auth, email confirmation OFF
- **FluentValidation 11** — validators registered via `AddValidatorsFromAssemblyContaining<Program>()`
- **xUnit** + `Microsoft.AspNetCore.Mvc.Testing`

## Architecture rules (enforce strictly)

- **Layered, not feature-organised**: `Controllers/`, `Pages/`, `Services/`, `Models/`, `Validators/`, `Common/`
- **No business logic in Controllers or Pages** — they call `Services/` only
- **Services** hold all business logic and own all DB access via `TaskCoDbContext`
- **Ownership**: every service query filters `.Where(x => x.OwnerId == _currentUser.UserId)`; cross-owner access returns `NotFound` error (never `Forbidden`)
- **OwnerId is never read from the client** — always taken from `ICurrentUser`
- When creating a `TaskItem`, load its parent `Project` filtered by owner, then copy `Project.OwnerId` onto the task

## Naming conventions

| Thing | Convention |
|---|---|
| Entity for tasks | `TaskItem` (never `Task`) |
| DTOs | `*Request` / `*Response` suffix |
| Interfaces | `I*` prefix |
| Async methods | `*Async` suffix |

## API response envelope

Success:
```json
{ "data": <payload> }
```
Error:
```json
{ "error": { "code": "...", "message": "...", "details": [...] } }
```

Status codes: `200` read/update · `201` create · `400` validation · `401` unauthenticated · `404` not-owned-or-missing

## Key source files

| File | Purpose |
|---|---|
| `src/TaskManager.Web/Program.cs` | DI wiring, middleware, DB provider selection |
| `src/TaskManager.Web/Common/Results/Result.cs` | `Result<T>` with implicit conversions |
| `src/TaskManager.Web/Common/Results/Error.cs` | `Error` record + `ErrorCodes` constants |
| `src/TaskManager.Web/Common/Api/ApiControllerBase.cs` | `OkEnvelope`, `CreatedEnvelope`, `FromResult`, `MapError` helpers |
| `src/TaskManager.Web/Services/Abstractions/ICurrentUser.cs` | `UserId`, `IsAuthenticated` |
| `src/TaskManager.Web/Data/TaskCoDbContext.cs` | EF Core context + model config |

## Running the app

```powershell
# First time only — apply migrations
cd src\TaskManager.Web
dotnet ef database update

# Run
dotnet run
# → http://localhost:5196
```

## Running tests

```powershell
# Must use Release — Windows AppControl blocks freshly-built Debug DLLs
dotnet test tests\TaskManager.Tests -c Release --output <any-output-path>
```

## Test architecture

- **Unit tests** — `InMemoryDatabase` + `FakeCurrentUser`; one fresh DB per test class via `Guid.NewGuid()` in constructor
- **Integration tests** — `CustomWebApplicationFactory` sets `Environment = "Test"` (triggers InMemory DB in `Program.cs`) and replaces auth with `TestAuthHandler`
- **`TestAuthHandler`** — reads `X-Test-UserId` header to impersonate any user per-request
- **`IntegrationTestBase`** — `IAsyncLifetime`; fresh factory + client per test instance; `SeedProjectAsync` / `SeedTaskItemAsync` helpers; `ParseData` / `ParseError` assert envelope shape
- **`OwnershipIsolationTests`** — proves cross-owner access always returns 404, never 403 or 200

## Known SQLite constraints

- `DateTimeOffset` columns cannot be used in EF Core `OrderBy` clauses with the SQLite provider — sort client-side after `ToListAsync()` instead

## Database

- Dev DB: `src/TaskManager.Web/taskmanager.db`
- Browse with **DB Browser for SQLite** (sqlitebrowser.org) — open the `.db` file, use the Browse Data tab
- Connection string in `appsettings.json`: `"Data Source=taskmanager.db"`
