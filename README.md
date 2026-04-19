# CEI

`CEI` is the Phase 1 foundation for **Corporativo Estrategia Integral**, a legal case management application built as a modular monolith on ASP.NET Core Blazor Server, SQL Server, EF Core, and ASP.NET Core Identity.

Phase 1 includes:

- Authentication with local Identity and seeded demo roles/users
- Role and permission foundation
- Dashboard with upcoming, overdue, and unread deadline reminders
- Case master records
- Structured case events and timeline view
- Deadline tracking with in-app reminders
- Document metadata plus secure local fake-file storage
- Audit logging
- SQL Server migrations
- Domain, application, infrastructure, and web separation

## Solution Structure

- `src/CEI.Domain`: entities, enums, and domain primitives
- `src/CEI.Application`: use-case services, DTOs, access logic, reminder scheduling
- `src/CEI.Infrastructure`: EF Core persistence, Identity, seeding, storage, background processing
- `src/CEI.Web`: Blazor UI, routing, authorization policies, login/logout endpoints
- `tests/CEI.Domain.Tests`
- `tests/CEI.Application.Tests`
- `tests/CEI.IntegrationTests`
- `scripts/Provision-Database.ps1`

## Architecture Summary

- Clean Architecture with a modular-monolith boundary
- SQL Server persistence with EF Core migrations
- ASP.NET Core Identity with seeded roles:
  - `PrincipalLawyer`
  - `Administrator`
  - `Assistant`
  - `Specialist`
- Permission-based authorization using `cases.*`, `documents.*`, `deadlines.*`, `audit.*`, and `users.*`
- Sensitive-case access protected by assignment/elevated permission logic
- Document access can be stricter than case access through confidentiality level and role grants
- Reminder subsystem runs in-process every 15 minutes and creates in-app notifications for:
  - 7 days before due date
  - 3 days before due date
  - 1 day before due date
  - due date
  - daily once overdue

## Local Setup

1. Restore and build:

```powershell
dotnet restore
dotnet build
```

2. Provision the SQL Server database and scoped app login if you need to recreate them:

```powershell
.\scripts\Provision-Database.ps1 `
  -AdminConnectionString "<admin-sql-connection-string>" `
  -DatabaseName "CEI" `
  -AppLogin "cei_app" `
  -AppLoginPassword "<scoped-app-password>"
```

3. Set local secrets for the web project:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<sql-connection-string>" --project .\src\CEI.Web
dotnet user-secrets set "Seed:DemoPassword" "<shared-demo-password>" --project .\src\CEI.Web
```

4. Create or update the database schema:

```powershell
dotnet dotnet-ef database update --project .\src\CEI.Infrastructure --startup-project .\src\CEI.Web
```

5. Run the application:

```powershell
dotnet run --project .\src\CEI.Web --launch-profile https
```

The HTTP profile uses `http://localhost:5138`.

## Demo Access

Seeded demo users:

- `principal@cei.local`
- `admin@cei.local`
- `assistant@cei.local`
- `specialist@cei.local`

The shared development password is stored only in local user secrets. To display it on this machine:

```powershell
dotnet user-secrets list --project .\src\CEI.Web
```

## Verification Flow

1. Open `/login`
2. Sign in with one of the seeded demo users
3. Create a new case from `Nuevo asunto`
4. Open the case detail page
5. Add:
   - at least one case event
   - one deadline
   - one fake PDF/document
6. Confirm:
   - the vertical timeline renders and links to the event section
   - the deadline appears in the dashboard widgets
   - the document preview loads through the secured document endpoint
   - reminder items appear when deadlines match the configured cadence

## Tests

Run all tests:

```powershell
dotnet test
```

Current automated coverage includes:

- deadline state transitions
- case creation and CEI code generation
- event chronology ordering
- reminder generation cadence
- sensitive case visibility behavior
- restricted document denial plus audit trail

## GitHub And Credentials

Configured remote:

- `https://github.com/Jorge-Contreras/cei.git`

Recommended workflow:

1. Keep the GitHub PAT out of the repo and out of `appsettings`
2. Let **Git Credential Manager** store it in Windows Credential Manager
3. Push normally:

```powershell
git push -u origin main
```

When Git prompts for GitHub credentials, use your GitHub username and PAT. Git Credential Manager will store the PAT securely on this machine.

## Important Development Notes

- Fake documents only: do not upload real client files until a dedicated security/go-live review is completed
- File bytes stay outside the repo under:
  - `C:\Users\Orion\AppData\Local\CEI\Storage\Development`
- SQL bootstrap/admin credentials should be used only for provisioning, not for normal app runtime
- The app runtime uses the scoped `cei_app` SQL login stored in local user secrets

## Recommended Next Phases

1. Event and deadline editing UI with richer audit diffs
2. Stronger assignment/access overlays by user and document
3. Deadline-rule assistants per matter/state template
4. Richer timeline visualization and printable procedural summaries
5. Safer production hosting split between migrator identity and runtime identity
6. Hardening for real document operations, malware scanning, retention, and formal audit review
