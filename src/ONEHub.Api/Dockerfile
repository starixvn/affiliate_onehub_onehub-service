# Stage 1: Build .NET Backend
FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine-25h2 AS builder
ARG PROJECT=src/ONEHub.Api/ONEHub.Api.csproj

WORKDIR /app

RUN dotnet nuget add source https://nuget.nexdev.net/api/v2

COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY ONEHubService.sln ./
COPY src/ONEHub.Shared/*.csproj ./src/ONEHub.Shared/
COPY src/ONEHub.Api/*.csproj ./src/ONEHub.Api/
COPY src/ONEHub.CronJob/*.csproj ./src/ONEHub.CronJob/

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "$PROJECT"

COPY src ./src

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "$PROJECT" \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Stage 2: Runtime Image
FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-alpine

# Don't run as root
RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app && chown -R dotnetuser /tmp
USER dotnetuser

WORKDIR "/app"

COPY --from=builder /app/publish .
ENV TZ="America/La_Paz"

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

# Start the application
ENTRYPOINT ["dotnet", "ONEHub.Api.dll"]
