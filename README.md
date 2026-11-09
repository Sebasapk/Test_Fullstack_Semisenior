# Task Manager

Internal task management system: create users, assign tasks to them and track their status (`Pending`, `InProgress`, `Done`). Each task can store additional information as JSON, validated and queried with SQL Server's native functions.

## Tech stack

- **Backend:** .NET 10 (SDK 10.0.401), ASP.NET Core Web API, Entity Framework Core 10, Swagger UI, xUnit
- **Frontend:** Angular 22, TypeScript 6.0
- **Database:** SQL Server 2016 or later (tested with 2025 Express and LocalDB)

## Getting started

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), [Node.js](https://nodejs.org/) `^22.22.3`, `^24.15` or `>=26`, SQL Server, and `sqlcmd` or SQL Server Management Studio. Run the commands in PowerShell from the repository root.

### 1. Database

Replace `[YourServer]` with your server, for example `localhost\SQLEXPRESS`, `localhost` or `"(localdb)\MSSQLLocalDB"`. You can also run the scripts in SQL Server Management Studio, in the same order.

```powershell
sqlcmd -S [YourServer] -E -C -i database\01_schema.sql
sqlcmd -S [YourServer] -E -C -i database\02_seed.sql
```

`01_schema.sql` creates the database and `02_seed.sql` adds optional sample data. `03_queries.sql` is also optional: it has example queries, should run after the seed and leaves the data unchanged. Re-running `01_schema.sql` or `02_seed.sql` deletes existing data. To use a SQL Server login, replace `-E` with `-U <user> -P <password>`.

### 2. API

Set your server in `backend/TaskManager.Api/appsettings.json`. This is the only configuration change needed. Backslashes are doubled in JSON, for example `localhost\\SQLEXPRESS`. To use a SQL Server login, replace `Trusted_Connection=True` with `User Id=<user>;Password=<password>`.

```json
"DefaultConnection": "Server=[YourServer];Database=TaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True"
```

```powershell
cd backend
dotnet run --project TaskManager.Api
```

The API runs on `http://localhost:5053`, with Swagger UI at `/swagger`. The frontend expects this port, and CORS only allows `http://localhost:4200`.

### 3. Frontend

In a second terminal, from the repository root:

```powershell
cd frontend
npm ci
npm start
```

If PowerShell blocks `npm` or `npx` with an execution policy error, use `npm.cmd` or `npx.cmd`.

Open `http://localhost:4200`.

### Tests

Run `dotnet test` in `backend` and `npx ng test --watch=false` in `frontend`.

## API reference

| Method | Route | Description | Success | Errors |
| --- | --- | --- | --- | --- |
| `POST` | `/api/users` | Create a user | `201` | `400`, `409` |
| `GET` | `/api/users` | List users | `200` | |
| `POST` | `/api/tasks` | Create a task | `201` | `400`, `404` |
| `GET` | `/api/tasks` | List tasks, newest first | `200` | `400` |
| `PUT` | `/api/tasks/{id}/status` | Change a task's status | `200` | `400`, `404` |

```http
POST /api/users

{ "name": "Jane Doe", "email": "jane.doe@example.com" }
```

```http
POST /api/tasks

{
  "title": "Prepare release notes",
  "description": "Summarize the changes.",
  "userId": 4,
  "additionalData": {
    "priority": "High",
    "estimatedEndDate": "2026-11-20",
    "tags": ["docs", "release"],
    "metadata": { "source": "web" }
  }
}
```

`title` and `userId` are required. New tasks start as `Pending`.

```http
GET /api/tasks?status=Pending&userId=4&priority=High
```

All filters are optional.

```http
PUT /api/tasks/11/status

{ "status": "InProgress" }
```

Business errors use the `ProblemDetails` format:

```json
{
  "title": "Bad request",
  "status": 400,
  "detail": "A task cannot go from Pending to Done directly; it must be InProgress first.",
  "instance": "/api/tasks/11/status"
}
```

## Technical decisions

### Layers

- **Api:** controllers, error handling and startup.
- **Application:** DTOs, services, business rules and repository interfaces.
- **Domain:** `User` and `TaskItem` entities and the `TaskStatus` enum.
- **Infrastructure:** `AppDbContext` and repository implementations.
- **Tests:** service unit tests.

The repository interfaces live in Application, so the business rules do not depend on Entity Framework and can be tested with fake repositories.

### EF Core without migrations

`database/01_schema.sql` owns the schema because a creation script is a required deliverable, and `AppDbContext` only maps to it. The trade-off is that a schema change must be made by hand in both places.

### Where the rules live

The services have the final say. The frontend gives faster feedback, and the database protects the data from anything that bypasses the API.

| Rule | Frontend | API | Database |
| --- | --- | --- | --- |
| Title is required | Form validator | DTO and `TaskService` | `NOT NULL` |
| Task has an existing user | User list | DTO and `TaskService` | Foreign key |
| No `Pending` to `Done` | Option not offered | `TaskService` | |
| Unique email | Checked against loaded users | `UserService` | `UNIQUE` |

`Pending` to `Done` is the only blocked transition, so a task can also move back.

### Error handling

Services throw business exceptions, and `GlobalExceptionHandler` maps them to `400`, `404` and `409`. Unexpected errors return a generic `500` and are only logged. Business errors use `ProblemDetails`; model validation errors use ASP.NET Core's `ValidationProblemDetails`. The frontend interceptor handles both and shows the message in a banner.

### Composite index

`IX_Tasks_UserId_Status_CreatedAt (UserId, Status, CreatedAt DESC)` follows the main query: a user's tasks, filtered by status, newest first. Filtering by status alone does not get a seek, because `UserId` is the leading column.

### JSON

`Tasks.AdditionalData` is an `NVARCHAR(MAX)` column with a `CHECK` constraint using `ISJSON`, so new properties need no schema change.

- **API:** `AdditionalData` is a `JsonElement` instead of a typed DTO, so a new property needs no backend change. The trade-off is that the API only checks that it is a JSON object, so clients must not assume its properties or types.
- **Filtering:** `GET /api/tasks?priority=High` uses `JSON_VALUE` through EF Core, so it runs in the database. No index backs this filter, which is acceptable at this scale.

The queries below use the sample data. `database/03_queries.sql` has similar examples.

```sql
-- ISJSON: validates text. The CHECK constraint uses it, so the INSERT fails with error 547.
SELECT Id, Title, ISJSON(AdditionalData) AS IsValidJson
FROM dbo.Tasks;

INSERT INTO dbo.Tasks (Title, UserId, AdditionalData)
VALUES (N'Invalid task', 1, N'{not json');

-- JSON_VALUE: reads a scalar value and filters by it.
SELECT Id, Title, JSON_VALUE(AdditionalData, '$.priority') AS Priority
FROM dbo.Tasks
WHERE JSON_VALUE(AdditionalData, '$.priority') = 'High';

-- JSON_QUERY: returns an array or object as JSON text.
SELECT Id, Title, JSON_QUERY(AdditionalData, '$.tags') AS Tags
FROM dbo.Tasks
WHERE AdditionalData IS NOT NULL;

-- OPENJSON: expands the tags array into rows to filter by a single tag.
SELECT t.Id, t.Title, tag.value AS Tag
FROM dbo.Tasks AS t
CROSS APPLY OPENJSON(t.AdditionalData, '$.tags') AS tag
WHERE tag.value = 'backend';

-- JSON_MODIFY: updates one property. Rolled back to keep the sample data intact.
BEGIN TRANSACTION;

UPDATE dbo.Tasks
SET AdditionalData = JSON_MODIFY(AdditionalData, '$.priority', 'Low')
WHERE Id = 1;

SELECT Id, Title, JSON_VALUE(AdditionalData, '$.priority') AS Priority
FROM dbo.Tasks
WHERE Id = 1;

ROLLBACK TRANSACTION;
```

## Pending features

All required features are implemented. Possible next steps:

- **Authentication and authorization** with JWT, so users only see their own tasks.
- **Pagination** for the task and user lists.
- **Editing and deleting** tasks and users.
- **A computed column with an index** on `JSON_VALUE(AdditionalData, '$.priority')`, if the priority filter is used often.
- **Docker Compose** to start SQL Server, the API and the frontend with one command.