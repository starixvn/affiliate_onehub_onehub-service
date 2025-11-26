using AspNetCoreRateLimit;
using ONEHub.Api;
using ONEHub.Api.Configurations;
using ONEHub.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencyInjection(builder.Configuration);

var app = builder.Build();

// Configure middleware pipeline
app.UseExceptionHandler();
app.UseIpRateLimiting();
app.UseSwaggerConfiguration(app.Environment);

// Map endpoints
app.MapHealthChecks("/healthz");
DataEndpoints.MapEndpoints(app);

app.Run();
