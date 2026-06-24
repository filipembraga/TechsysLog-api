using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TechsysLog.API.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock = new();

    [Fact]
    public async Task InvokeAsync_WithoutExistingHeader_GeneratesNewCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Response.Headers["X-Correlation-Id"].ToString();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithExistingHeader_PreservesIncomingCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = "client-generated-id";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers["X-Correlation-Id"].ToString().Should().Be("client-generated-id");
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        var middleware = new CorrelationIdMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }
}