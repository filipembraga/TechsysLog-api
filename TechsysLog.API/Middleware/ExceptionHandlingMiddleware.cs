using System.Net;
using System.Text.Json;

namespace TechsysLog.API.Middleware;

/// <summary>
/// Global exception handler middleware.
///
/// Design decision: exceptions are caught here rather than in each controller
/// to avoid repetitive try/catch blocks and ensure consistent error responses
/// across the entire API.
///
/// Mapping strategy:
/// - InvalidOperationException   → 409 Conflict (business rule violation)
/// - KeyNotFoundException        → 404 Not Found (resource does not exist)
/// - UnauthorizedAccessException → 401 Unauthorized (authentication failure)
/// - All others                  → 500 Internal Server Error
///
/// Stack traces are never exposed in responses — only a safe message is returned.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            InvalidOperationException ex  => (HttpStatusCode.Conflict, ex.Message),
            KeyNotFoundException ex       => (HttpStatusCode.NotFound, ex.Message),
            UnauthorizedAccessException _ => (HttpStatusCode.Unauthorized, "Invalid credentials."),
            _                             => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            statusCode = (int)statusCode,
            message
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}