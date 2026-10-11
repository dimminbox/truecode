using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.Map("/health", health => health.Run(context =>
{
    context.Response.StatusCode = StatusCodes.Status200OK;
    return Task.CompletedTask;
}));

if (app.Environment.IsDevelopment())
{
    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapScalarApiReference(options => options
            .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
            .AddDocuments(["users", "finance"]));
        endpoints.MapGet("/", () => Results.Content(ApiIndexPage, "text/html; charset=utf-8"))
            .ExcludeFromDescription();
    });
}

await app.UseOcelot();

app.Run();

partial class Program
{
    private const string ApiIndexPage =
        """
        <!doctype html>
        <html lang="ru">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <title>TrueCode API</title>
            <style>
                body { font-family: system-ui, sans-serif; max-width: 480px; margin: 12vh auto; padding: 0 16px; }
                a { display: block; padding: 16px 20px; margin: 12px 0; border: 1px solid #8884; border-radius: 8px;
                    text-decoration: none; color: inherit; }
                a:hover { background: #8881; }
                small { display: block; opacity: .6; margin-top: 4px; }
            </style>
        </head>
        <body>
            <h1>TrueCode API</h1>
            <a href="/scalar/users">Users<small>Регистрация, логин, refresh, logout</small></a>
            <a href="/scalar/finance">Finance<small>Курсы валют</small></a>
        </body>
        </html>
        """;
}
