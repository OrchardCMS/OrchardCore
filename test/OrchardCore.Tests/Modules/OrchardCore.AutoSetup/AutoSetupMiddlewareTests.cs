using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.AutoSetup.Options;
using OrchardCore.AutoSetup.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Setup.Services;
using Xunit;
using OrchardCore.AutoSetup;
using OrchardCore.AutoSetup.Extensions;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.AutoSetup;

public class AutoSetupMiddlewareTests
{
    private readonly Mock<IShellHost> _mockShellHost;
    private readonly ShellSettings _shellSettings;
    private readonly Mock<IShellSettingsManager> _mockShellSettingsManager;
    private readonly Mock<IDistributedLock> _mockDistributedLock;
    private readonly Mock<IOptions<AutoSetupOptions>> _mockOptions;
    private readonly Mock<IAutoSetupService> _mockAutoSetupService;

    public AutoSetupMiddlewareTests()
    {
        _mockShellHost = new Mock<IShellHost>();
        _shellSettings = new ShellSettings();
        _mockShellSettingsManager = new Mock<IShellSettingsManager>();
        _mockDistributedLock = new Mock<IDistributedLock>();
        _mockOptions = new Mock<IOptions<AutoSetupOptions>>();
        _mockAutoSetupService = new Mock<IAutoSetupService>();

        _mockOptions.Setup(o => o.Value).Returns(new AutoSetupOptions
        {
            LockOptions = new LockOptions(),
            Tenants = new List<TenantSetupOptions>
                {
                    new TenantSetupOptions { ShellName = ShellSettings.DefaultShellName }
                }
        });
    }    

    [Fact]
    public async Task InvokeAsync_InitializedShell_SkipsSetup()
    {
        // Arrange
        _shellSettings.State = TenantState.Running;

        var httpContext = new DefaultHttpContext();
        var nextCalled = false;

        var middleware = new AutoSetupMiddleware(
            next: (innerHttpContext) => { nextCalled = true; return Task.CompletedTask; },
            _mockShellHost.Object,
            _shellSettings,
            _mockShellSettingsManager.Object,
            _mockDistributedLock.Object,
            _mockOptions.Object);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.True(nextCalled);
        _mockAutoSetupService.Verify(s => s.SetupTenantAsync(It.IsAny<TenantSetupOptions>(), It.IsAny<ShellSettings>()), Times.Never);
    }    
}

