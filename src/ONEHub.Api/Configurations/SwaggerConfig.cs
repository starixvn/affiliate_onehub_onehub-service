using Microsoft.OpenApi.Models;

namespace ONEHub.Api.Configurations;

/// <summary>
/// Swagger/OpenAPI configuration extensions
/// </summary>
public static class SwaggerConfig
{
    public static IServiceCollection AddSwaggerConfiguration(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ONEHub API",
                Version = "v1",
                Description = "Public API for accessing synchronized data",
                Contact = new OpenApiContact
                {
                    Name = "ONEHub Team"
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(
        this IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}
