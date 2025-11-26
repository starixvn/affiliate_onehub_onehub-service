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
- Implement **dependency injection** for all services
- Apply **interface-based programming** for testability
- Use **minimal APIs** or controller-based approach consistently

### Architecture Patterns
- **Clean Architecture**: Separation of concerns with dedicated layers
- **Centralized DI Orchestration**: `DependencyInjection.cs` at project root consolidates all service registrations with a single `AddDependencyInjection()` call from `Program.cs`
- **Feature-Based Configuration**: Each feature (Redis, Hangfire, Polly, etc.) has dedicated configuration extension methods in `Configurations/` folder
- **Repository Pattern**: Abstract data access via shared interfaces
- **Retry Pattern**: Use Polly for transient fault handling when calling external APIs
- **Background Job Pattern**: Hangfire for scheduled data pulls, jobs inherit from `JobBase` abstract class
- **Rate Limiting**: Protect public API endpoints from abuse
- **Shared Kernel**: Common code in `ONEHub.Shared` project

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

**Current Status:**
- **CronJob**: Fully implemented centralized pattern
- **API**: Has `DependencyInjection.cs` file but `Program.cs` still uses individual configuration method calls

### Polly Configuration
```csharp
// Retry policy for external API calls in cronjob
services.AddHttpClient("ExternalApi")
    .AddTransientHttpErrorPolicy(builder => 
        builder.WaitAndRetryAsync(new[]
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(10)
        }));
```

### Rate Limiting Configuration
```json
// appsettings.json for ONEHub.Api
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "HttpStatusCode": 429,
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

### Hangfire Configuration
- **Storage**: Redis for distributed job storage
- **Dashboard**: Secure with authentication
- **Recurring Jobs**: Define in `Program.cs` or startup
- **Job Retry**: Configure automatic retry on failure

### Testing Strategy
- **Unit Tests**: Test business logic in isolation
- **Integration Tests**: Test API endpoints and job execution
- **Mocking**: Use Moq or NSubstitute for dependencies
- **Test Coverage**: Aim for >80% coverage on critical paths

### Git Workflow
- **Main branch**: Production-ready code
- **Develop branch**: Integration branch for features
- **Feature branches**: `feature/feature-name`
- **Commit conventions**: Use conventional commits (feat, fix, chore, etc.)

## Domain Context
This project synchronizes data from external sources on a scheduled basis and exposes that data through a rate-limited public API. The cronjob component periodically pulls data using resilient HTTP calls (with retry), while the API component serves the synchronized data to consumers.

## Important Constraints
- **Resilience**: All external API calls must use Polly retry policies
- **Rate Limiting**: Public API must enforce rate limits to prevent abuse
- **Containerization**: Each project (CronJob and Api) must have its own Dockerfile
- **Job Persistence**: Hangfire jobs must use Redis for distributed scenarios
- **Code Reuse**: Common code must reside in the Shared project

## External Dependencies
- **External Data API**: Third-party API for data pulling (configure endpoint in appsettings)
- **Redis**: Required for Hangfire job storage and caching
- **Docker**: For containerization and deployment
