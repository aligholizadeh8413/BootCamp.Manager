# BootCamp.Manager

An instructor dashboard for the third C# Bootcamp session assignment, featuring a Persian RTL interface, ASP.NET Core 10 Controllers, EF Core 10, and SQLite.

This project continues the learning path from the previous [CSharp.CRUD.Practice](https://github.com/aligholizadeh8413/CSharp.CRUD.Practice) exercise, moving from Console CRUD to HTTP. The `session3.pptx` slides define the intended learning level. The project uses four small layers and deliberately avoids React, TypeScript, MediatR, AutoMapper, and Docker.

## Getting Started on macOS

1. Install **.NET 10 SDK** from the [official .NET 10 download page](https://dotnet.microsoft.com/en-us/download/dotnet/10.0): choose Arm64 for Apple Silicon or x64 for Intel Macs. The Runtime alone is not sufficient. `global.json` allows a stable 10.0 SDK with a newer feature band. If you only have SDK 11 installed, install SDK 10 alongside it.

2. Extract the ZIP archive and open Terminal in the directory containing `BootCamp.Manager.sln`:

```bash
cd BootCamp.Manager
dotnet --list-sdks
dotnet tool restore
dotnet restore --disable-parallel -p:BuildInParallel=false
dotnet build -m:1
```

3. Apply the migration and run the application:

```bash
dotnet ef database update \
  --project src/BootCamp.Manager.Infrastructure \
  --startup-project src/BootCamp.Manager.WebApi \
  --no-build

dotnet run --no-build --project src/BootCamp.Manager.WebApi
```

Dashboard: **http://localhost:5083** — MVC example: **http://localhost:5083/learning/mvc**

On the first run, the database is empty. Add an instructor and students first, then create a session. The application also runs `MigrateAsync` at startup, so the `database update` command is provided to demonstrate and practice the migration process; running it again is safe. This behavior is intended for local learning.

To populate an empty database with sample records, run the following command in a second terminal (with Python 3 installed):

```bash
python3 scripts/seed-demo.py
```

The sample data is inserted into SQLite through real POST requests; it is separate from the browser-based sample data in `docs/`. The script runs only when all main tables are empty. The application does not require login and is configured for this local exercise.

## Features and Rules

- Full CRUD for students, instructors, sessions, and evaluations; attendance can be recorded and updated.
- Each session can have an assignment for the next session and a course progress value from 0 to 100.
- Session participants are selected and saved when the session is created. Adding or deactivating a student does not change the statistics of previous sessions.
- Attendance percentage = present students ÷ total participants in that session × 100. An unrecorded status is different from Present. An empty session returns `null` and is displayed as “—” in the UI.
- Evaluations include a score from 0 to 100, comments, and the evaluating instructor. Each student can have at most one evaluation per session.
- A person with historical records cannot be deleted (`409`); deactivate them instead. Deleting a session also deletes its attendance and evaluations.
- Modal forms, card pagination, the supplied Rokh font, SVG icons, and no emojis.
- Browser date inputs use the standard Gregorian format; dates are displayed in Persian using `Intl`.

## Learning Guide

| File | Learning content |
|---|---|
| [Architecture and diagrams](documentation/01-architecture.md) | Project structure, layer dependencies, ERD, and request flow |
| [Step-by-step learning guide](documentation/02-learning-guide.md) | File responsibilities, EF, DI, DTOs, Controllers, and comparison with FastAPI |
| [Complete API reference](documentation/03-api.md) | All endpoints, JSON payloads, and errors |
| [Middleware, Filters, and Forms](documentation/04-web-concepts.md) | Conceptual differences and a testable MVC example |
| [Running and migrations](documentation/05-running.md) | macOS, SQL, GitHub Pages, and troubleshooting |
| [Actual test report](documentation/06-test-report.md) | Test results and verification limitations |
| [BootCamp.Manager.http](BootCamp.Manager.http) | CRUD request sequence using an HTTP Client |
| [schema.sql](documentation/schema.sql) | Actual SQL generated from the migration |

Suggested reading order: `Student` → `StudentRequest` → `StudentsController` → `StudentService` → `StudentRepository` → `BootcampDbContext` → `Program.cs` → `wwwroot/js/data.js`.

## Run the Tests Again

```bash
dotnet build -m:1
python3 tests/smoke.py
node tests/demo.mjs
```

The Python test starts the real server on an available port, creates a temporary database, checks CRUD operations and restart persistence, and then deletes the temporary database. Node is only used for optional JavaScript tests; running the application itself does not require Node or npm.

## GitHub Pages Demo

`docs/` is a standalone version. It does not send `/api` requests; sample data is stored in the current browser's `localStorage`. A notice at the top of the page indicates that the demo is not connected to the backend. The reset button restores the sample data.

```bash
python3 -m http.server 8080 --directory docs
```

Then open `http://localhost:8080`. Do not open the page using `file://`, because JavaScript modules require HTTP.

To publish the demo, push the files to the [BootCamp.Manager repository](https://github.com/aligholizadeh8413/BootCamp.Manager), then go to Settings → Pages. Select **Deploy from a branch**, choose the `main` branch, and set the folder to `/docs`. Having the folder ready does not mean GitHub Pages has been published; this project package has not been pushed to the repository yet.

## Simplification Decisions

Pagination is handled in the browser; the API returns the full list. This exercise targets the relatively small amount of data expected in a bootcamp. Services are registered directly with DI, while repositories have interfaces. We have not created an additional interface for every class or a separate Unit of Work because `DbContext` already fulfills that persistence role. EF-generated migration files are intentionally kept in source control.
