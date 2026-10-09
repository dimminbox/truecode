using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TrueCode.Finance.Api.ExceptionHandling;

internal sealed class GlobalExceptionHandler
    : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment environment)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var isDevelopment = _environment.IsDevelopment();

        _logger.LogError(
            exception,
            "Unhandled exception. Path: {Path}, Method: {Method}, User: {User}",
            httpContext.Request.Path,
            httpContext.Request.Method,
            httpContext.User.Identity?.Name ?? "Anonymous");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Type = exception.GetType().Name,
                Title = isDevelopment ? exception.Message : "an error occured",
                Detail = isDevelopment ? exception.StackTrace : null,
                Status = StatusCodes.Status500InternalServerError,
            }
        });
    }
}
