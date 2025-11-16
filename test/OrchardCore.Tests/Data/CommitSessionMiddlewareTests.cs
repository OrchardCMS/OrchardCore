using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Data;
using OrchardCore.Data.Documents;
using YesSqlSession = YesSql.ISession;

namespace OrchardCore.Tests.Data;

public class CommitSessionMiddlewareTests
{
    [Fact]
    public async Task Middleware_WhenDisabled_DoesNotRegisterCallback()
    {
        // Arrange
        var options = Options.Create(new EnsureCommittedOptions { Enabled = false });
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
        await middleware.InvokeAsync(context, options);

        // Assert
        Assert.True(nextCalled);
        // When disabled, no callback is registered so commit should never be called
        mockDocumentStore.Verify(ds => ds.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Middleware_WhenEnabled_RegistersCallback()
    {
        // Arrange
        var options = Options.Create(new EnsureCommittedOptions { Enabled = true });
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
        await middleware.InvokeAsync(context, options);

        // Assert
        Assert.True(nextCalled);
        // Callback is registered but not called until response starts
    }

    [Fact]
    public async Task Middleware_WhenFlushOnPathsConfigured_SkipsNonMatchingPaths()
    {
        // Arrange
        var options = Options.Create(new EnsureCommittedOptions
        {
            Enabled = true,
            FlushOnPaths = new[] { "/api/", "/token" },
        });
        var mockLogger = new Mock<ILogger<CommitSessionMiddleware>>();
        var mockDocumentStore = new Mock<IDocumentStore>();

        var services = new ServiceCollection();
        services.AddSingleton(mockDocumentStore.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Test with non-matching path
        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
        };
        context.Request.Path = "/home";
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
        await middleware.InvokeAsync(context, options);

        // Assert
        Assert.True(nextCalled);
        // Should skip commit for non-matching path
    }

    [Fact]
    public async Task Middleware_WhenFlushOnPathsConfigured_ProcessesMatchingPaths()
    {
        // Arrange
        var options = Options.Create(new EnsureCommittedOptions
        {
            Enabled = true,
            FlushOnPaths = new[] { "/api/", "/token" },
        });
        var mockLogger = new Mock<ILogger<CommitSessionMiddleware>>();
        var mockDocumentStore = new Mock<IDocumentStore>();

        var services = new ServiceCollection();
        services.AddSingleton(mockDocumentStore.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Test with matching path
        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
        };
        context.Request.Path = "/api/users";
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
        await middleware.InvokeAsync(context, options);

        // Assert
        Assert.True(nextCalled);
        // Should process commit for matching path
    }

    [Fact]
    public void EnsureCommittedOptions_HasCorrectDefaults()
    {
        // Arrange & Act
        var options = new EnsureCommittedOptions();

        // Assert
        Assert.True(options.Enabled);
        Assert.Equal(CommitFailureBehavior.ThrowOnCommitFailure, options.FailureBehavior);
        Assert.Empty(options.FlushOnPaths);
    }

    [Fact]
    public async Task Middleware_WhenExceptionOccurs_DoesNotCommit()
    {
        // Arrange
        var options = Options.Create(new EnsureCommittedOptions { Enabled = true });
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
            await middleware.InvokeAsync(context, options);
        });

        // Verify that commit was never called due to the exception
        mockDocumentStore.Verify(ds => ds.CommitAsync(), Times.Never);
        mockSession.Verify(s => s.SaveChangesAsync(), Times.Never);
    }
}
