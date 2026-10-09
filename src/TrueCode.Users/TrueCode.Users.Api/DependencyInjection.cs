using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using TrueCode.Users.Api.ExceptionHandling;
using TrueCode.Users.Api.Security;

namespace TrueCode.Users.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddApiToken(configuration);
        services.AddApiDocumentation();
        services.AddErrorHandling();
        return services;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseApiDocumentation();
        app.UseHttpsRedirection();
        app.MapControllers();
        return app;
    }

    private static IServiceCollection AddApiToken(this IServiceCollection services, IConfiguration configuration)
    {
        var token = configuration["Auth:ApiToken"];
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException(
                "Не задан Auth:ApiToken. Укажите его в appsettings или переменной окружения Auth__ApiToken.");

        services.AddSingleton(new ApiTokenFilter(token));
        return services;
    }

    private static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "TrueCode.Users", Version = "v1" });
            options.AddSecurityDefinition(ApiTokenDefaults.SecuritySchemeName, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = ApiTokenDefaults.HeaderName,
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = ApiTokenDefaults.SecuritySchemeName,
                    }
                }] = []
            });
        });
        return services;
    }

    private static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }

    private static WebApplication UseApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.UseSwagger();
        app.MapScalarApiReference(options =>
            options.WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json"));
        app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
        return app;
    }
}
