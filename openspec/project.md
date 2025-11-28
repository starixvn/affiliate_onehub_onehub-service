# Project Context

## Summary
ONEHub Service is a .NET 9 data synchronization solution consisting of:
- **ONEHub.CronJob**: Hangfire-based background service for scheduled data pulling from external APIs
- **ONEHub.Api**: Public REST API with rate limiting to serve synchronized data
- **ONEHub.Shared**: Common library for DTOs, interfaces, helpers, and constants

The system uses Redis for job persistence and caching, Polly for HTTP resilience, and follows interface-based dependency injection patterns throughout.

## Purpose
A .NET 9 solution for data synchronization and API provisioning. The system pulls data from external APIs via scheduled jobs and provides a public API to serve that data.

## Tech Stack
- **.NET 9** - Primary framework
- **Hangfire** - Background job processing with Redis storage
- **Redis** - Job storage and caching
- **Polly** - Resilience and transient-fault-handling (retry policies)
- **AspNetCoreRateLimit** - API rate limiting protection
- **Docker** - Containerization (separate containers for each project)

## Project Structure

```
onehub-service/
├── src/
│   ├── ONEHub.Shared/              # Shared library (DTOs, utilities, interfaces)
│   │   ├── Services/               # ICacheService and implementations
│   │   ├── Utilities/              # Helper utilities (LoggerUtils, etc.)
│   │   └── ONEHub.Shared.csproj
│   │
│   ├── ONEHub.CronJob/             # Hangfire cronjob project
│   │   ├── Jobs/                   # Background job classes (inherit from JobBase)
│   │   ├── Schedulers/             # JobScheduler for Hangfire recurring jobs
│   │   ├── Configurations/         # Feature configs (Redis, Hangfire, Polly)
│   │   ├── DependencyInjection.cs  # Centralized DI orchestration
│   │   ├── GlobalExceptionHandler.cs
│   │   ├── Program.cs
│   │   ├── Dockerfile
│   │   └── appsettings.json
│   │
│   └── ONEHub.Api/                 # Public API project
│       ├── Endpoints/              # Minimal API endpoint definitions
│       ├── Configurations/         # Feature configs (Redis, RateLimit, Polly, Swagger)
│       ├── DependencyInjection.cs  # Centralized DI orchestration (not yet used in Program.cs)
│       ├── GlobalExceptionHandler.cs
│       ├── Program.cs
│       ├── Dockerfile
│       └── appsettings.json
│
├── tests/
│   ├── ONEHub.Shared.Tests/
│   ├── ONEHub.CronJob.Tests/
│   └── ONEHub.Api.Tests/
│
└── docker-compose.yml              # Orchestrate all containers
```

## Project Conventions

### Code Style
- Follow **C# coding conventions** and naming guidelines
- Use **async/await** for all I/O operations
- Implement **dependency injection** for all services via interface-based programming
- Use **minimal APIs** with endpoint grouping (not controllers)
- Primary constructor syntax preferred for dependency injection (C# 12+)

### Architecture Patterns
- **Centralized DI Orchestration**: `DependencyInjection.cs` at project root consolidates all service registrations with a single `AddDependencyInjection()` call from `Program.cs`
- **Feature-Based Configuration**: Each feature (Redis, Hangfire, Polly, etc.) has dedicated configuration extension methods in `Configurations/` folder
- **Repository Pattern**: Use Fanex.Data.Repository for all database access with stored procedures defined in `SpNames.xml`
- **Retry Pattern**: Use Polly for transient fault handling when calling external APIs (configured in `PollyConfig.cs`)
- **Background Job Pattern**: Hangfire for scheduled data pulls
  - Jobs inherit from `JobBase` abstract class
  - Job settings in `appsettings.json` (not hardcoded)
  - Automatic trigger endpoints at `/api/jobs/trigger/{JobName}`
- **Rate Limiting**: AspNetCoreRateLimit protects API endpoints (IP-based throttling)
- **Shared Kernel**: Common code in `ONEHub.Shared` project (services, utilities)
- **Secrets Management**: HashiCorp Vault for sensitive configuration

### Dependency Injection Pattern
The solution uses a two-tier approach:

**Tier 1: Centralized Orchestration**
- `DependencyInjection.cs` at project root (same level as `Program.cs`)
- Single extension method: `AddDependencyInjection(IConfiguration)`
- Orchestrates all service registrations in one place
- `Program.cs` makes single call: `builder.Services.AddDependencyInjection(builder.Configuration)`

**Tier 2: Feature-Specific Configuration**
- Configuration classes in `Configurations/` folder
- Examples: `RedisConfig.cs`, `HangfireConfig.cs`, `PollyConfig.cs`, `RateLimitConfig.cs`
- Each provides extension methods like `AddRedisConfiguration()`, `AddHangfireConfiguration()`
- Called from `DependencyInjection.AddDependencyInjection()`

**Implementation Status:**
- Both CronJob and Api projects use centralized DI pattern
- `Program.cs` makes single call to `AddDependencyInjection(configuration)`

### Polly Configuration
HTTP clients are configured with retry policies in `PollyConfig.cs`:
```csharp
services.AddHttpClientWithPolicies(configuration);  // Extension method
```
Transient HTTP errors trigger exponential backoff retries.
Proxy support available via `ExternalApi:Proxy` configuration.

### Job Configuration
Jobs are configured in `appsettings.json` under `JobSettings` array:
```json
{
  "JobSettings": [
    {
      "Name": "CustomerPullJob",
      "CronSchedule": "1/30 * * * *",  // Every 30 minutes
      "AdjustmentValue": "-30",
      "MaxTimeRange": "30",
      "DataSource": "MemberInfoLastImportedTime",
      "MaxBatchSize": 100
    }
  ]
}
```
Jobs are scheduled automatically via `JobConfig.UseCronJobs()` middleware.
Each job also gets a manual trigger endpoint.

### Database Access Pattern
- Use Fanex.Data.Repository for all database operations
- Define stored procedures in `SpNames.xml`
- Connection strings as array in `appsettings.json` with Name, ConnectionString, ProviderName
- Repository interfaces: `IDynamicRepository`, `IDataRepository`, `IDataTrackerRepository`
- All repositories registered as singletons

### Hangfire Dashboard
- Dashboard path configured via `HangfireSettings:DashboardPath`
- Dashboard secret key for authentication via `HangfireSettings:DashboardSecretKey`
- Redis storage for job persistence (shared with caching)
- Accessible at configured path (default: `/hangfire`)

## Domain Context
This project synchronizes data from external sources on a scheduled basis and exposes that data through a rate-limited public API. The cronjob component periodically pulls data using resilient HTTP calls (with retry), while the API component serves the synchronized data to consumers.

## Important Constraints
- **Resilience**: All external API calls must use Polly retry policies configured in `PollyConfig.cs`
- **Rate Limiting**: Public API enforces IP-based rate limits (60/min, 1000/hour default)
- **Database Access**: Must use Fanex.Data.Repository with stored procedures in `SpNames.xml`
- **Job Configuration**: Job settings in `appsettings.json`, not hardcoded
- **Redis Required**: Both CronJob (Hangfire) and Api (rate limiting) require Redis
- **Job Pattern**: All jobs inherit from `JobBase` and implement `ExecuteAsync()`
- **DI Pattern**: Use centralized `DependencyInjection.cs` with feature-specific configurations
- **Code Reuse**: Common services in `ONEHub.Shared` project

## External Dependencies
- **SQL Server**: Primary database (connection via Fanex.Data.Repository)
- **Redis**: Required for Hangfire job storage and rate limiting
- **HashiCorp Vault**: Optional secrets management (can be disabled)
- **Fanex Data Service API**: External API service (configurable endpoint)
- **Docker**: For containerization and deployment
