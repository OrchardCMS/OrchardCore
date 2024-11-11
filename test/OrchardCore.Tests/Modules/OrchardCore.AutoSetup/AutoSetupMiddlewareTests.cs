using OrchardCore.AutoSetup;
using OrchardCore.AutoSetup.Options;
using OrchardCore.AutoSetup.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Setup.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.AutoSetup;

public class AutoSetupMiddlewareTests
{
    private readonly Mock<IShellHost> _mockShellHost;
    private readonly ShellSettings _shellSettings;
    private readonly Mock<IShellSettingsManager> _mockShellSettingsManager;
    private readonly Mock<IDistributedLock> _mockDistributedLock;
    private readonly Mock<IOptions<AutoSetupOptions>> _mockOptions;
    private readonly Mock<IAutoSetupService> _mockAutoSetupService;
    private bool _nextCalled;
    public AutoSetupMiddlewareTests()
    {
        _shellSettings = new ShellSettings();
        _shellSettings.AsDefaultShell();
        _mockShellHost = new Mock<IShellHost>();
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

        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.True(_nextCalled);
        _mockAutoSetupService.Verify(s => s.SetupTenantAsync(It.IsAny<TenantSetupOptions>(), It.IsAny<ShellSettings>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_FailedSetup_ReturnsServiceUnavailable()
    {
        // Arrange
        _shellSettings.State = TenantState.Uninitialized;

        SetupDistributedLockMock(true);

        var setupContext = new SetupContext { Errors = new Dictionary<string, string> { { "Error", "Test error" } } };
        _mockAutoSetupService.Setup(s => s.SetupTenantAsync(It.IsAny<TenantSetupOptions>(), It.IsAny<ShellSettings>()))
            .ReturnsAsync((setupContext, false));

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection()
            .AddSingleton(_mockAutoSetupService.Object)
            .BuildServiceProvider();

        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_UnInitializedShell_PerformsSetup()
    {
        // Arrange
        _shellSettings.State = TenantState.Uninitialized;

        SetupDistributedLockMock(true);

        var setupContext = new SetupContext();
        _mockAutoSetupService.Setup(s => s.SetupTenantAsync(It.IsAny<TenantSetupOptions>(), It.IsAny<ShellSettings>()))
            .ReturnsAsync((setupContext, true));

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection()
            .AddSingleton(_mockAutoSetupService.Object)
            .BuildServiceProvider();

        var middleware = CreateMiddleware();
        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, httpContext.Response.StatusCode); // Redirect
        _mockAutoSetupService.Verify(s => s.SetupTenantAsync(It.IsAny<TenantSetupOptions>(), It.IsAny<ShellSettings>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_FailedLockAcquisition_ThrowsTimeoutException()
    {
        // Arrange
        _shellSettings.State = TenantState.Uninitialized;

        SetupDistributedLockMock(false);

        var httpContext = new DefaultHttpContext();

        var middleware = CreateMiddleware();

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => middleware.InvokeAsync(httpContext));
    }

    private void SetupDistributedLockMock(bool acquireLock)
    {
        var mockLocker = new Mock<ILocker>();
        _mockDistributedLock
            .Setup(d => d.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync((mockLocker.Object, acquireLock));
    }

    private AutoSetupMiddleware CreateMiddleware()
    {
        return new AutoSetupMiddleware(
                    next: (innerHttpContext) =>
                    {
                        _nextCalled = true;

                        return Task.CompletedTask;
                    },
                    _mockShellHost.Object,
                    _shellSettings,
                    _mockShellSettingsManager.Object,
                    _mockDistributedLock.Object,
                    _mockOptions.Object);
    }
}
