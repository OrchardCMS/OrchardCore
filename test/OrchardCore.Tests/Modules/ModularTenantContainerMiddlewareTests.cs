using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Clusters;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Modules;

public class ModularTenantContainerMiddlewareTests
{
    [Fact]
    public async Task Invoke_NoClusterMatchesTenant_ReturnsServiceUnavailable()
    {
        // Arrange
        var shellSettings = new ShellSettings
        {
            Name = "Tenant1",
        };

        shellSettings.VersionId = "tenant-1";

        var shellHost = new Mock<IShellHost>();
        shellHost.Setup(host => host.InitializeAsync()).Returns(Task.CompletedTask);

        var runningShellTable = new Mock<IRunningShellTable>();
        runningShellTable
            .Setup(table => table.Match(It.IsAny<HostString>(), It.IsAny<PathString>(), true))
            .Returns(shellSettings);

        var clustersOptionsMonitor = new Mock<IOptionsMonitor<ClustersOptions>>();
        clustersOptionsMonitor.SetupGet(monitor => monitor.CurrentValue).Returns(new ClustersOptions
        {
            Enabled = true,
        });

        var nextCalled = false;
        var middleware = new ModularTenantContainerMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            shellHost.Object,
            runningShellTable.Object,
            clustersOptionsMonitor.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        // Act
        await middleware.Invoke(httpContext);

        // Assert
        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, httpContext.Response.StatusCode);

        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body);
        Assert.Equal("No cluster is configured for the requested tenant.", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Invoke_RequestFromClustersProxy_DoesNotExposeTheSecretToTheTenant(bool enabled)
    {
        // Arrange
        var shellSettings = new ShellSettings
        {
            Name = "Tenant1",
        };

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[RequestHeaderNames.FromClustersProxy] = "secret";

        var headerAtTenantScope = "not-called";

        var shellHost = new Mock<IShellHost>();
        shellHost.Setup(host => host.InitializeAsync()).Returns(Task.CompletedTask);
        shellHost
            .Setup(host => host.GetScopeAsync(shellSettings))
            .Callback(() => headerAtTenantScope = httpContext.Request.Headers[RequestHeaderNames.FromClustersProxy].ToString())
            .ThrowsAsync(new OperationCanceledException());

        var runningShellTable = new Mock<IRunningShellTable>();
        runningShellTable
            .Setup(table => table.Match(It.IsAny<HostString>(), It.IsAny<PathString>(), true))
            .Returns(shellSettings);

        var clustersOptionsMonitor = new Mock<IOptionsMonitor<ClustersOptions>>();
        clustersOptionsMonitor.SetupGet(monitor => monitor.CurrentValue).Returns(new ClustersOptions
        {
            Enabled = enabled,
            ProxySecret = "secret",
        });

        var middleware = new ModularTenantContainerMiddleware(
            _ => Task.CompletedTask,
            shellHost.Object,
            runningShellTable.Object,
            clustersOptionsMonitor.Object);

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(() => middleware.Invoke(httpContext));

        // Assert
        Assert.Empty(headerAtTenantScope);
    }

    [Fact]
    public async Task RouterInvoke_NoTenantScope_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = new ModularTenantRouterMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<ModularTenantRouterMiddleware>.Instance);

        // Act
        await middleware.Invoke(new DefaultHttpContext());

        // Assert
        Assert.True(nextCalled);
    }
}
