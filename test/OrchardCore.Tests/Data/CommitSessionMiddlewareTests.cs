using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Data;
using OrchardCore.Data.Documents;
using YesSqlSession = YesSql.ISession;

namespace OrchardCore.Tests.Data;

public class CommitSessionMiddlewareTests
{
    [Fact]
    public async Task Middleware_RegistersCallback()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CommitSessionMiddleware>>();
        var mockDocumentStore = new Mock<IDocumentStore>();

        var services = new ServiceCollection();
        services.AddSingleton(mockDocumentStore.Object);
        var serviceProvider = services.BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
        };
        context.Items["OrchardCore:DocumentStoreResolved"] = true;

        var nextCalled = false;
        var middleware = new CommitSessionMiddleware(
            next: (innerHttpContext) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            logger: mockLogger.Object
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        // Callback is registered but not called until response starts
    }

    [Fact]
    public async Task Middleware_WhenExceptionOccurs_DoesNotCommit()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CommitSessionMiddleware>>();
        var mockDocumentStore = new Mock<IDocumentStore>();
        var mockSession = new Mock<YesSqlSession>();

        var services = new ServiceCollection();
        services.AddSingleton(mockDocumentStore.Object);
        services.AddSingleton(mockSession.Object);
        var serviceProvider = services.BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
        };
        context.Items["OrchardCore:SessionResolved"] = true;

        var middleware = new CommitSessionMiddleware(
            next: (innerHttpContext) =>
            {
                // Simulate an exception during request processing
                throw new InvalidOperationException("Test exception");
            },
            logger: mockLogger.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await middleware.InvokeAsync(context);
        });

        // Verify that commit was never called due to the exception
        mockDocumentStore.Verify(ds => ds.CommitAsync(), Times.Never);
        mockSession.Verify(s => s.SaveChangesAsync(), Times.Never);
    }
}
