# ONEHub Service

A .NET 9 solution for data synchronization and API provisioning. The system pulls data from external APIs via scheduled jobs and provides a public API to serve that data.

## Architecture

This solution consists of three projects:

### ONEHub.Shared
Common library containing:
- **Models**: Data transfer objects (DTOs)
- **Interfaces**: Service abstractions
- **Helpers**: Utility classes (HTTP client helpers)
- **Constants**: Application-wide constants

### ONEHub.CronJob
Background job service featuring:
- **Hangfire** for job scheduling with Redis storage
- **Polly** retry policies for resilient HTTP calls
- Scheduled data pulling from external APIs
- Health check endpoint at `/healthz`
- Hangfire dashboard at `/hangfire`

### ONEHub.Api
Public REST API featuring:
- **Minimal APIs** for lightweight endpoint definition
- **Rate limiting** to protect against abuse (60 req/min, 1000 req/hour)
- **Swagger/OpenAPI** documentation
- Health check endpoint at `/healthz`
- Global exception handling middleware

## Technology Stack

- **.NET 9** - Primary framework
- **Hangfire + Redis** - Background job processing
- **Polly** - Resilience and retry policies
- **AspNetCoreRateLimit** - API rate limiting
- **Swashbuckle** -OpenAPI/Swagger
- **Docker** - Containerization

## Package Management

This solution uses **Central Package Management** via `Directory.Packages.props` to manage NuGet package versions centrally.

## Getting Started

### Prerequisites
- .NET 9 SDK
- Redis (for Hangfire)
- Docker (optional, for containerized deployment)

### Running Locally

**Restore and build:**
```bash
dotnet restore
dotnet build
```

**Run CronJob:**
```bash
cd src/ONEHub.CronJob
dotnet run
```
- Hangfire Dashboard: http://localhost:5000/hangfire
- Health Check: http://localhost:5000/healthz

**Run Api:**
```bash
cd src/ONEHub.Api
dotnet run
```
- Swagger UI: http://localhost:5001/swagger
- Health Check: http://localhost:5001/healthz

### Running with Docker

**Build CronJob:**
```bash
docker build -t onehub-cronjob:latest -f src/ONEHub.CronJob/Dockerfile .
docker run -d -p 5000:8080 --name cronjob onehub-cronjob:latest
```

**Build Api:**
```bash
docker build -t onehub-api:latest -f src/ONEHub.Api/Dockerfile .
docker run -d -p 5001:8080 --name api onehub-api:latest
```

## Configuration

### CronJob Configuration
Edit `src/ONEHub.CronJob/appsettings.json`:
- `ConnectionStrings:Redis` - Redis connection string
- `ExternalApi:BaseUrl` - External API endpoint
- `ExternalApi:ApiKey` - API authentication key
- `ExternalApi:Proxy:Enabled` - Enable/disable proxy (true/false)
- `ExternalApi:Proxy:Host` - Proxy server address (e.g., "http://proxy.example.com:8080")

### Api Configuration
Edit `src/ONEHub.Api/appsettings.json`:
- `IpRateLimiting:GeneralRules` - Rate limit rules

## Project Structure

See [openspec/project.md](openspec/project.md) for detailed project conventions and architecture patterns.

## Health Checks

Both projects expose health check endpoints at `/healthz`:
- CronJob: Checks Redis connectivity
- Api: Basic health check

## Development

### Code Style
- Follow C# coding conventions (.NET)
- Use async/await for I/O operations
- Implement dependency injection for all services
- Use interface-based programming

### Job Configuration
Jobs are configured in `ONEHub.Shared/Constants/AppConstants.cs`:
```csharp
public const string DataPullCron = "0 */5 * * * *"; // Every 5 minutes
```

## License

All rights reserved.
