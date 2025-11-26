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

### Dependency Injection Pattern
The solution uses a two-tier DI setup:

**1. Centralized Orchestration (Root Level)**
- `DependencyInjection.cs` at project root (same level as `Program.cs`)
- Single `AddDependencyInjection(IConfiguration)` extension method that orchestrates all service registrations
- `Program.cs` makes a single call: `builder.Services.AddDependencyInjection(builder.Configuration)`
- Currently implemented in **CronJob** project; **API** project has the file but still uses individual calls in `Program.cs`

**2. Feature-Specific Configuration (Configurations Folder)**
- Each feature has dedicated extension method in `Configurations/` folder
- Examples: `AddRedisConfiguration()`, `AddHangfireConfiguration()`, `AddPollyPolicies()`, `AddRateLimitConfiguration()`
- Called from `DependencyInjection.AddDependencyInjection()` method

**CronJob DependencyInjection.cs structure:**
```csharp
public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
{
    services.AddHealthChecks();
    services.AddRedisConfiguration(configuration);

    var redis = services.BuildServiceProvider().GetRequiredService<IConnectionMultiplexer>();

    services.AddHangfireConfiguration(redis);
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
- Jobs are scheduled in `JobScheduler.ScheduleRecurringJobs()` using Hangfire's `RecurringJob.AddOrUpdate()`
- Job IDs and cron expressions defined in `AppConstants.Jobs` (referenced in README but not yet implemented)

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
- **Fanex.Data.Repository** - Data access abstraction

## Configuration Files

### CronJob appsettings.json
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"  // Hangfire storage + caching
  },
  "ExternalApi": {
    "BaseUrl": "https://api.example.com",
    "ApiKey": "your-api-key",
    "Proxy": {
      "Enabled": false,
      "Host": "http://proxy.example.com:8080"
    }
  }
}
```

### API appsettings.json
```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 60
      }
    ]
  }
}
```

## Development Patterns

### Adding New Background Jobs
1. Create job class inheriting from `JobBase` in `ONEHub.CronJob/Jobs/`
2. Implement `ExecuteAsync()` method with business logic
3. Register job in `JobScheduler.ScheduleRecurringJobs()`
4. Define cron expression and job ID in `AppConstants.Jobs`

### Adding New API Endpoints
1. Create endpoint class in `ONEHub.Api/Endpoints/`
2. Define static `MapEndpoints(WebApplication app)` method
3. Use endpoint grouping with `.MapGroup("/api/resource")`
4. Call mapping method from `Program.cs`
5. Add service interfaces and implementations as needed

### Adding New Services
1. Define interface in `ONEHub.Shared/Services/` (if shared) or project-specific folder
2. Implement service class
3. Register in `DependencyInjection.cs` (either in main method or create a private helper method like `AddCronJobs()`)
4. Use constructor injection to consume service

Example:
```csharp
// In DependencyInjection.cs
private static IServiceCollection AddCronJobs(this IServiceCollection services)
{
    services.AddScoped<IMyService, MyService>();
    return services;
}
```

## Health Checks

Both projects expose `/healthz` endpoints:
- **CronJob**: Checks Redis connectivity (required for Hangfire)
- **API**: Basic health check (expand as needed)

## Important Constraints

- All external API calls MUST use Polly retry policies
- All services MUST use interface-based DI
- All I/O operations MUST use async/await
- Redis is REQUIRED for CronJob (Hangfire storage)
- Rate limiting is enforced on all API endpoints

## Related Documentation

See `openspec/project.md` for detailed architecture patterns and conventions.
