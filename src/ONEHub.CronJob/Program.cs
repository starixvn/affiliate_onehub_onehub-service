using ONEHub.CronJob;
using ONEHub.CronJob.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencyInjection(builder.Configuration);

var app = builder.Build();

// Configure middleware
app.UseExceptionHandler();

app.UseHangfireConfiguration();

// Map endpoints
app.MapHealthChecks("/healthz");
app.MapGet("/", () => "Hello world!");

// Schedule recurring jobs
app.UseCronJobs();

app.Run();
