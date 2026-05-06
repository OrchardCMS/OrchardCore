using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Clusters;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Modules;

public class ModularTenantContainerMiddlewareTests
{
    [Fact]
    public async Task InvokeReturnsServiceUnavailableWhenNoClusterMatchesResolvedTenant()
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
}
