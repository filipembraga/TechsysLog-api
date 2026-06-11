using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TechsysLog.API.Middleware;

namespace TechsysLog.Tests.Middleware;

/// <summary>
/// Tests for the global exception handling middleware.
///
/// Test philosophy (Simple Testing Can Prevent Most Critical Failures — Yuan et al.):
/// 92% of catastrophic failures originate from incorrect error handling.
/// These tests deliberately cover every exception branch and verify that:
///   - each exception type maps to the correct HTTP status code
///   - internal details (stack traces, raw exception messages) never reach the client
///   - errors are logged and NOT silently swallowed
///   - the happy path is unaffected
/// </summary>
public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock = new();

    private ExceptionHandlingMiddleware Build(RequestDelegate next)
        => new(next, _loggerMock.Object);

    private static DefaultHttpContext CreateContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    private static async Task<(int StatusCode, JsonElement Body)> ReadResponse(HttpContext ctx)
    {
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var raw = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        return (ctx.Response.StatusCode, JsonDocument.Parse(raw).RootElement);
    }

    // ── Happy path ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNextAndLeavesStatusUntouched()
    {
        var called = false;
        var middleware = Build(_ => { called = true; return Task.CompletedTask; });
        var ctx = CreateContext();

        await middleware.InvokeAsync(ctx);

        called.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(200);
    }

    // ── Business rule violation ─────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WhenInvalidOperationException_Returns409WithOriginalMessage()
    {
        const string msg = "Order ORD-00001 has already been delivered.";
        var middleware = Build(_ => throw new InvalidOperationException(msg));
        var ctx = CreateContext();

        await middleware.InvokeAsync(ctx);
        var (status, body) = await ReadResponse(ctx);

        status.Should().Be(409);
        body.GetProperty("statusCode").GetInt32().Should().Be(409);
        body.GetProperty("message").GetString().Should().Be(msg);
    }

    // ── Resource not found ──────────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WhenKeyNotFoundException_Returns404WithOriginalMessage()
    {
        const string msg = "Order abc123 not found.";
        var middleware = Build(_ => throw new KeyNotFoundException(msg));
        var ctx = CreateContext();

        await middleware.InvokeAsync(ctx);
        var (status, body) = await ReadResponse(ctx);

        status.Should().Be(404);
        body.GetProperty("statusCode").GetInt32().Should().Be(404);
        body.GetProperty("message").GetString().Should().Be(msg);
    }

    // ── Authentication failure ──────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessException_Returns401WithSafeMessage()
    {
        // The original exception message may contain internal token details.
        // The middleware MUST replace it with a generic safe message.
        var middleware = Build(_ => throw new UnauthorizedAccessException("User identity not found in token."));
        var ctx = CreateContext();

        await middleware.InvokeAsync(ctx);
        var (status, body) = await ReadResponse(ctx);

        status.Should().Be(401);
        body.GetProperty("statusCode").GetInt32().Should().Be(401);
        body.GetProperty("message").GetString().Should().Be("Invalid credentials.");
        body.GetProperty("message").GetString().Should().NotContain("token");
    }

    // ── Unhandled / unexpected exception ───────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_Returns500WithSafeMessage()
    {
        var middleware = Build(_ => throw new Exception("Connection pool exhausted — internal detail"));
        var ctx = CreateContext();

        await middleware.InvokeAsync(ctx);
        var (status, body) = await ReadResponse(ctx);

        status.Should().Be(500);
        body.GetProperty("statusCode").GetInt32().Should().Be(500);
        body.GetProperty("message").GetString().Should().Be("An unexpected error occurred.");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_DoesNotLeakInternalDetailsToClient()
    {
        // Stack traces and raw exception messages must NEVER reach the client.
        var middleware = Build(_ => throw new NullReferenceException("null ref at InternalService.Process"));
        var ctx = CreateContext();

        await middleware.InvokeAsync(ctx);
        var (_, body) = await ReadResponse(ctx);

        var responseText = body.GetRawText();
        responseText.Should().NotContain("null ref");
        responseText.Should().NotContain("InternalService");
        responseText.Should().NotContain("StackTrace");
        responseText.Should().NotContain("at TechsysLog");
    }

    // ── Response format ─────────────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WhenException_SetsContentTypeToJson()
    {
        var middleware = Build(_ => throw new Exception());
        var ctx = CreateContext();

        await middleware.InvokeAsync(ctx);

        ctx.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_WhenException_ResponseBodyUseCamelCaseProperties()
    {
        var middleware = Build(_ => throw new Exception());
        var ctx = CreateContext();

        await middleware.InvokeAsync(ctx);
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var raw = await new StreamReader(ctx.Response.Body).ReadToEndAsync();

        // Verify camelCase serialization — must have "statusCode" not "StatusCode"
        raw.Should().Contain("\"statusCode\"");
        raw.Should().Contain("\"message\"");
        raw.Should().NotContain("\"StatusCode\"");
        raw.Should().NotContain("\"Message\"");
    }

    // ── Error is logged, not silently swallowed ─────────────────────────────────

    [Fact]
    public async Task InvokeAsync_WhenException_LogsErrorWithExceptionInstance()
    {
        var exception = new Exception("something broke");
        var middleware = Build(_ => throw exception);
        var ctx = CreateContext();

        await middleware.InvokeAsync(ctx);

        // Verify the error was logged — a handler that ONLY logs is considered to ignore the error,
        // but here the log is paired with a proper response (not a swallow). Still, logging must happen.
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
