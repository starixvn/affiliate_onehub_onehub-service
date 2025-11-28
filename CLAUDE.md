# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

<!-- OPENSPEC:START -->
# OpenSpec Instructions

These instructions are for AI assistants working in this project.

Always open `@/openspec/AGENTS.md` when the request:
- Mentions planning or proposals (words like proposal, spec, change, plan)
- Introduces new capabilities, breaking changes, architecture shifts, or big performance/security work
- Sounds ambiguous and you need the authoritative spec before coding

Use `@/openspec/AGENTS.md` to learn:
- How to create and apply change proposals
- Spec format and conventions
- Project structure and guidelines

Keep this managed block so 'openspec update' can refresh the instructions.

<!-- OPENSPEC:END -->

## Project Overview

ONEHub Service is a .NET 9 data synchronization solution with two main components:
1. **ONEHub.CronJob** - Hangfire-based background service that pulls data from external APIs on a schedule
2. **ONEHub.Api** - Public REST API that serves synchronized data with rate limiting
3. **ONEHub.Shared** - Common library containing DTOs, interfaces, helpers, and constants

## Common Commands

### Build and Run
```bash
# Restore packages and build entire solution
dotnet restore
dotnet build

# Run CronJob service (Hangfire dashboard at http://localhost:5000/hangfire)
cd src/ONEHub.CronJob
dotnet run

# Run API service (Swagger at http://localhost:5001/swagger)
cd src/ONEHub.Api
dotnet run

# Run specific project from solution root
dotnet run --project src/ONEHub.CronJob
dotnet run --project src/ONEHub.Api
```

### Docker
```bash
# Build and run CronJob container
docker build -t onehub-cronjob:latest -f src/ONEHub.CronJob/Dockerfile .
docker run -d -p 5000:8080 --name cronjob onehub-cronjob:latest

# Build and run API container
docker build -t onehub-api:latest -f src/ONEHub.Api/Dockerfile .
docker run -d -p 5001:8080 --name api onehub-api:latest
```

### Testing
```bash
# Run all tests (when test projects exist)
dotnet test

# Run tests for specific project
dotnet test tests/ONEHub.Shared.Tests
```

## Architecture Overview

### Database Layer (Fanex.Data.Repository)
The solution uses Fanex.Data.Repository for database access:
- All data access goes through repository interfaces (`IDynamicRepository`, `IDataRepository`, `IDataTrackerRepository`)
- Stored procedures are defined in `SpNames.xml` at the project root
- Connection strings configured in `appsettings.json` as array of connection objects
- Supports multiple database connections with different credentials
- Configured via `AddFanexData(configuration)` in DependencyInjection

### Secrets Management (Vault)
HashiCorp Vault integration for secure secret storage:
- Configured via `VaultOptions` in appsettings.json
- Enable/disable with `Enable` flag
- Automatically polls for secret updates based on `MinutesPolling`
- Used for database credentials and sensitive configuration

### Dependency Injection Pattern
The solution uses a two-tier DI setup:

**1. Centralized Orchestration (Root Level)**
- `DependencyInjection.cs` at project root (same level as `Program.cs`)
- Single `AddDependencyInjection(IConfiguration)` extension method that orchestrates all service registrations
- `Program.cs` makes a single call: `builder.Services.AddDependencyInjection(builder.Configuration)`
- Implemented in **both CronJob and Api** projects

**2. Feature-Specific Configuration (Configurations Folder)**
- Each feature has dedicated extension method in `Configurations/` folder
- Examples: `AddRedisConfiguration()`, `AddHangfireConfiguration()`, `AddPollyPolicies()`, `AddRateLimitConfiguration()`
- Called from `DependencyInjection.AddDependencyInjection()` method

**CronJob DependencyInjection.cs structure:**
```csharp
public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
{
    // Register and validate options
    services.Configure<JobSettings>(configuration.GetSection(nameof(JobSettings)));
    services.Configure<RedisSettings>(configuration.GetSection(nameof(RedisSettings)));
    services.Configure<HangfireSettings>(configuration.GetSection(nameof(HangfireSettings)));

    services.AddHealthChecks();
    services.AddFanexData(configuration);  // Database layer
    services.AddRedisConfiguration(redisSettings);
    services.AddHangfireConfiguration(redisSettings);
    services.AddHttpClientWithPolicies(configuration);
    services.AddExceptionHandler<GlobalExceptionHandler>();
    services.AddProblemDetails();
    services.AddCronJobs();  // Private method for job-specific registrations

    return services;
}
```

### Job System Architecture
Background jobs inherit from `JobBase` abstract class:
- `JobBase.RunAsync()` handles logging and exception handling
- Concrete jobs implement `ExecuteAsync()` for business logic
- Jobs are scheduled in `JobConfig.UseCronJobs()` using Hangfire's `RecurringJob.AddOrUpdate<T>()`
- Job configuration (cron schedule, data source, batch size) defined in `appsettings.json` under `JobSettings` array
- Each job gets a manual trigger endpoint at `/api/jobs/trigger/{JobName}` for on-demand execution

### HTTP Client + Polly Resilience
All external API calls use HttpClient configured with Polly retry policies:
- Configured in `PollyConfig.cs` files in both CronJob and Api projects
- Transient HTTP errors trigger exponential backoff retries (typically 1s, 5s, 10s)
- Proxy support available via `ExternalApi:Proxy` configuration

### Cache Service Pattern
Redis caching abstracted through `ICacheService` interface:
- Implementation: `RedisCacheService` in ONEHub.Shared
- Supports generic `GetAsync<T>`, `SetAsync<T>`, `RemoveAsync`, `ExistsAsync`
- Configured via `RedisConfig.cs` with health checks

### API Endpoint Pattern
Uses Minimal APIs with endpoint grouping:
- Endpoints defined in static classes (e.g., `DataEndpoints.MapEndpoints()`)
- Services injected via method parameters
- Grouped by feature with tags for Swagger
- Rate limiting applied globally via `AspNetCoreRateLimit`

### Exception Handling
Global exception handlers via `IExceptionHandler`:
- `GlobalExceptionHandler` in both projects
- Registered with `AddExceptionHandler<T>()` and `AddProblemDetails()`
- Middleware applied via `UseExceptionHandler()`

## Package Management

This solution uses **Central Package Management** via `Directory.Packages.props`:
- All package versions defined centrally at solution root
- `.csproj` files reference packages WITHOUT version attributes
- To update a package version, edit `Directory.Packages.props`

Key packages:
- **Hangfire.Redis.StackExchange** - Job persistence
- **Polly** and **Microsoft.Extensions.Http.Polly** - Resilience
- **AspNetCoreRateLimit** - API throttling
- **Fanex.Data.Repository** - Data access abstraction with stored procedure support
- **Fanex.Data.ProxyClient** - HTTP proxy support for external API calls
- **StackExchange.Redis** - Redis client for caching

## Configuration Files

### CronJob appsettings.json
```json
{
  "StoredProcedurePath": "SpNames.xml",
  "FanexDataServiceApi": "http://fanexdataservices.nexdev.net",
  "ConnectionStrings": [
    {
      "Name": "DBOH.ONEHub.wsv_oh",
      "ConnectionString": "Data Source=...;Initial Catalog=ONEHub;...",
      "ProviderName": "System.Data.SqlClient"
    }
  ],
  "JobSettings": [
    {
      "Name": "CustomerPullJob",
      "AdjustmentValue": "-30",
      "CronSchedule": "1/30 * * * *",
      "MaxTimeRange": "30",
      "DataSource": "MemberInfoLastImportedTime",
      "MaxBatchSize": 100
    }
  ],
  "RedisSettings": {
    "InstanceName": "ONEHub_CronJob_",
    "Configuration": "localhost:6379,defaultDatabase=5"
  },
  "HangfireSettings": {
    "DashboardSecretKey": "your-secret-key",
    "DashboardPath": "/hangfire"
  },
  "VaultOptions": {
    "Enable": "true",
    "Address": "https://vault.example.com",
    "Role": "role-id",
    "SecretId": "secret-id"
  }
}
```

### API appsettings.json
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 60
      },
      {
        "Endpoint": "*",
        "Period": "1h",
        "Limit": 1000
      }
    ]
  }
}
```

## Development Patterns

### Working with Stored Procedures
1. Define stored procedure names in [SpNames.xml](src/ONEHub.CronJob/SpNames.xml)
2. Use repository interfaces to execute:
   ```csharp
   var result = await _dynamicRepository.QueryAsync<T>(
       spName: "YourStoredProcName",
       parameters: new { param1 = value1 }
   );
   ```
3. Repositories are registered as singletons in `DependencyInjection.AddCronJobs()`

### Manual Job Triggering
Jobs can be triggered manually via HTTP endpoints:
- Endpoint: `POST /api/jobs/trigger/{JobName}`
- Example: `POST /api/jobs/trigger/CustomerPullJob`
- Returns job ID and timestamp
- Useful for testing and on-demand execution

### Adding New Background Jobs
1. Create job class inheriting from `JobBase` in [ONEHub.CronJob/Jobs/](src/ONEHub.CronJob/Jobs/)
2. Implement `ExecuteAsync()` method with business logic
3. Register job in `DependencyInjection.AddCronJobs()` private method
4. Add job scheduling in [JobConfig.cs:55](src/ONEHub.CronJob/Configurations/JobConfig.cs#L55) via `ScheduleJob<YourJob>(jobSettings)`
5. Add trigger endpoint in [JobConfig.cs:60](src/ONEHub.CronJob/Configurations/JobConfig.cs#L60) via `MapJobTriggerEndpoint<YourJob>(app)`
6. Define job configuration in `appsettings.json` under `JobSettings` array with cron schedule, data source, etc.

### Adding New API Endpoints
1. Create endpoint class in [ONEHub.Api/Endpoints/](src/ONEHub.Api/Endpoints/)
2. Define static `MapEndpoints(WebApplication app)` method
3. Use endpoint grouping with `.MapGroup("/api/resource")` and `.WithTags()` for Swagger organization
4. Call mapping method from [Program.cs:19](src/ONEHub.Api/Program.cs#L19)
5. Add service interfaces and implementations as needed in `DependencyInjection.cs`

### Adding New Services
1. Define interface in [ONEHub.Shared/Services/Interfaces/](src/ONEHub.Shared/Services/Interfaces/) (if shared) or project-specific folder
2. Implement service class in [ONEHub.Shared/Services/](src/ONEHub.Shared/Services/) or project folder
3. Register in `DependencyInjection.cs`:
   - For shared services: Register in both CronJob and Api `DependencyInjection.cs`
   - For job-specific services: Use private `AddCronJobs()` helper in CronJob
4. Use constructor injection to consume service

Example:
```csharp
// In CronJob/DependencyInjection.cs private method
services.AddSingleton<ILoggerService, LoggerService>();
services.AddSingleton<ICacheService, RedisCacheService>();
services.AddScoped<IExternalApiClient, ExternalApiClient>();
```

## Health Checks

Both projects expose `/healthz` endpoints:
- **CronJob**: Checks Redis connectivity (required for Hangfire)
- **API**: Basic health check (expand as needed)

## Important Constraints

- All external API calls MUST use Polly retry policies
- All services MUST use interface-based DI
- All I/O operations MUST use async/await
- Redis is REQUIRED for both CronJob (Hangfire storage) and Api (rate limiting)
- Rate limiting is enforced on all API endpoints (60/min, 1000/hour by default)
- Database access MUST use Fanex.Data.Repository with stored procedures defined in `SpNames.xml`
- Job configuration MUST be in `appsettings.json` under `JobSettings` array, not hardcoded
- All new jobs MUST inherit from `JobBase` and implement `ExecuteAsync()`

## Related Documentation

See `openspec/project.md` for detailed architecture patterns and conventions.
