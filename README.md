# Business Processes Automation — Back-end

ASP.NET Core 8 API + Telegram bot for master booking automation.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express, or full instance on `localhost`)
- Telegram bot token from [@BotFather](https://t.me/BotFather) (for the bot)

## Quick start

### 1. Clone and open the solution

```bash
cd Business-Processes-Automation
# Open Business-Processes-Automation.sln in IDE, or work from the project folder below
```

Project path:

`Business-Processes-Automation/Business-Processes-Automation/`

### 2. Local secrets (not committed to git)

Create `appsettings.Development.local.json` in the project folder (this file is gitignored):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BusinessProcessesAutomationDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  },
  "Telegram": {
    "BotToken": "paste-your-bot-token-from-botfather",
    "UsePolling": true,
    "BotUsername": "your_bot_username_without_at"
  }
}
```

Fill in:

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string (optional if default in `appsettings.json` is fine) |
| `Telegram:BotToken` | Bot token from BotFather (**required** for Telegram) |
| `Telegram:BotUsername` | Bot username without `@` |

### 3. Database

From the project folder:

```bash
dotnet ef database update
```

This applies all EF Core migrations to `BusinessProcessesAutomationDb` (or your database from the connection string).

### 4. Run

```bash
dotnet run 
```

- Swagger UI: https://localhost:7049/swagger  
- HTTP: http://localhost:5021  

### 5. Auth API (optional)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register master (web) |
| POST | `/api/auth/login` | Login (sets cookie) |
| GET | `/api/auth/me` | Current user profile |
| POST | `/api/auth/logout` | Logout |

## Configuration files

| File | Purpose |
|------|---------|
| `appsettings.json` | Shared defaults: DB connection template, logging, Telegram polling flag |
| `appsettings.Development.json` | Development overrides (committed): e.g. `BotUsername` placeholder |
| `appsettings.Development.local.json` | **Your machine only** (gitignored): token, optional DB override |

Later keys override earlier ones: `json` → `Development.json` → `Development.local.json`.

## Useful commands

```bash
# Build
dotnet build

# New migration after model changes
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update
```

## Project layout

```
Back-end/
├── Business-Processes-Automation.sln
├── README.md
└── Business-Processes-Automation/
    └── Business-Processes-Automation/   # main web project
        ├── BLL/                         # business logic
        ├── DAL/                         # entities, EF, migrations
        ├── Telegram/                    # bot handlers, keyboards
        ├── UI/Controllers/              # HTTP API (auth)
        ├── appsettings*.json
        └── Program.cs
```
