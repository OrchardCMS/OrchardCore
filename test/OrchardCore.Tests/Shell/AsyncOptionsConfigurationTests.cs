using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Tests.Stubs;
using StartupBase = OrchardCore.Modules.StartupBase;

namespace OrchardCore.Tests.Shell;

public class AsyncOptionsConfigurationTests
{
    private static readonly ShellSettings _uninitializedDefaultShell = new ShellSettings()
        .AsDefaultShell()
        .AsUninitialized();

    private readonly ShellContainerFactory _shellContainerFactory;

    public AsyncOptionsConfigurationTests()
    {
        var applicationServices = new ServiceCollection();
        applicationServices.AddSingleton<ITypeFeatureProvider, TypeFeatureProvider>();
        applicationServices.AddLogging();

        var loggerMock = new Mock<ILogger<ShellContainerFactory>>();

        _shellContainerFactory = new ShellContainerFactory(
            new StubHostingEnvironment(),
            new StubExtensionManager(),
            applicationServices.BuildServiceProvider(),
            applicationServices,
            loggerMock.Object
        );
    }

    [Fact]
    public async Task AsyncConfiguredOptions_AreResolvableViaIOptions()
    {
        // Arrange
        var shellBlueprint = CreateBlueprint();
        AddStartup(shellBlueprint, typeof(AsyncOptionsStartup));

        // Act
        var container = await _shellContainerFactory.CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint);

        // Run initializers (simulating what ShellContextFactory does)
        var shellContainerOptions = container.GetRequiredService<IOptions<ShellContainerOptions>>().Value;
        foreach (var initializeAsync in shellContainerOptions.Initializers)
        {
            await initializeAsync(container);
        }

        using var scope = container.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<TestAsyncOptions>>().Value;

        // Assert - options should be resolvable via IOptions<T>
        Assert.NotNull(options);
        Assert.Equal("AsyncConfigured", options.Value);
    }

    [Fact]
    public async Task AsyncConfiguration_RunsDuringInitialization_NotAtRegistration()
    {
        // Arrange
        var shellBlueprint = CreateBlueprint();
        AddStartup(shellBlueprint, typeof(TrackingAsyncOptionsStartup));

        // Reset tracking
        TrackingAsyncOptionsStartup.ConfigurationExecuted = false;

        // Act - Create container (registration happens here)
        var container = await _shellContainerFactory.CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint);

        // Assert - configuration should NOT have run yet
        Assert.False(TrackingAsyncOptionsStartup.ConfigurationExecuted, "Async configuration should not run at registration time");

        // Act - Run initializers (simulating tenant initialization on first request)
        var shellContainerOptions = container.GetRequiredService<IOptions<ShellContainerOptions>>().Value;
        foreach (var initializeAsync in shellContainerOptions.Initializers)
        {
            await initializeAsync(container);
        }

        // Assert - configuration should have run now
        Assert.True(TrackingAsyncOptionsStartup.ConfigurationExecuted, "Async configuration should run during initialization");
    }

    [Fact]
    public async Task IAsyncConfigureOptions_IsInvokedDuringInitialization()
    {
        // Arrange
        var shellBlueprint = CreateBlueprint();
        AddStartup(shellBlueprint, typeof(AsyncConfigureOptionsInterfaceStartup));

        // Reset tracking
        TestAsyncConfigureOptions.ConfigureAsyncCalled = false;

        // Act
        var container = await _shellContainerFactory.CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint);

        // Run initializers
        var shellContainerOptions = container.GetRequiredService<IOptions<ShellContainerOptions>>().Value;
        foreach (var initializeAsync in shellContainerOptions.Initializers)
        {
            await initializeAsync(container);
        }

        using var scope = container.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<TestAsyncOptions>>().Value;

        // Assert
        Assert.True(TestAsyncConfigureOptions.ConfigureAsyncCalled, "IAsyncConfigureOptions.ConfigureAsync should be invoked");
        Assert.Equal("ConfiguredByInterface", options.Value);
    }

    [Fact]
    public async Task MultipleAsyncConfigurators_AllExecute()
    {
        // Arrange
        var shellBlueprint = CreateBlueprint();
        AddStartup(shellBlueprint, typeof(MultipleConfiguratorStartup));

        // Act
        var container = await _shellContainerFactory.CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint);

        // Run initializers
        var shellContainerOptions = container.GetRequiredService<IOptions<ShellContainerOptions>>().Value;
        foreach (var initializeAsync in shellContainerOptions.Initializers)
        {
            await initializeAsync(container);
        }

        using var scope = container.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<CountingOptions>>().Value;

        // Assert - both configurators should have incremented the count
        Assert.Equal(2, options.ConfigurationCount);
    }

    [Fact]
    public async Task SyncAndAsyncConfiguration_BothApply()
    {
        // Arrange
        var shellBlueprint = CreateBlueprint();
        AddStartup(shellBlueprint, typeof(MixedConfigurationStartup));

        // Act
        var container = await _shellContainerFactory.CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint);

        // Run initializers
        var shellContainerOptions = container.GetRequiredService<IOptions<ShellContainerOptions>>().Value;
        foreach (var initializeAsync in shellContainerOptions.Initializers)
        {
            await initializeAsync(container);
        }

        using var scope = container.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<MixedOptions>>().Value;

        // Assert - both sync and async configurations should be applied
        Assert.Equal("SyncValue", options.SyncConfiguredValue);
        Assert.Equal("AsyncValue", options.AsyncConfiguredValue);
    }

    [Fact]
    public async Task PostConfigureAsync_RunsAfterConfigureAsync()
    {
        // Arrange
        var shellBlueprint = CreateBlueprint();
        AddStartup(shellBlueprint, typeof(ConfigureAndPostConfigureStartup));

        // Act
        var container = await _shellContainerFactory.CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint);

        // Run initializers
        var shellContainerOptions = container.GetRequiredService<IOptions<ShellContainerOptions>>().Value;
        foreach (var initializeAsync in shellContainerOptions.Initializers)
        {
            await initializeAsync(container);
        }

        using var scope = container.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<OrderTrackingOptions>>().Value;

        // Assert - PostConfigureAsync should run after ConfigureAsync
        Assert.Equal(2, options.ExecutionOrder.Count);
        Assert.Equal("ConfigureAsync", options.ExecutionOrder[0]);
        Assert.Equal("PostConfigureAsync", options.ExecutionOrder[1]);
    }

    [Fact]
    public async Task IPostAsyncConfigureOptions_IsInvokedAfterAsyncConfiguration()
    {
        // Arrange
        var shellBlueprint = CreateBlueprint();
        AddStartup(shellBlueprint, typeof(PostAsyncConfigureOptionsInterfaceStartup));

        // Reset tracking
        TestPostAsyncConfigureOptions.PostConfigureAsyncCalled = false;

        // Act
        var container = await _shellContainerFactory.CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint);

        // Run initializers
        var shellContainerOptions = container.GetRequiredService<IOptions<ShellContainerOptions>>().Value;
        foreach (var initializeAsync in shellContainerOptions.Initializers)
        {
            await initializeAsync(container);
        }

        using var scope = container.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<TestAsyncOptions>>().Value;

        // Assert
        Assert.True(TestPostAsyncConfigureOptions.PostConfigureAsyncCalled, "IPostAsyncConfigureOptions.PostConfigureAsync should be invoked");
        Assert.Equal("PostConfigured", options.Value);
    }

    [Fact]
    public async Task AsyncConfiguredOptions_WorkWithNestedScopes()
    {
        // Arrange
        var shellBlueprint = CreateBlueprint();
        AddStartup(shellBlueprint, typeof(AsyncOptionsStartup));

        // Act
        var container = await _shellContainerFactory.CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint);

        // Run initializers
        var shellContainerOptions = container.GetRequiredService<IOptions<ShellContainerOptions>>().Value;
        foreach (var initializeAsync in shellContainerOptions.Initializers)
        {
            await initializeAsync(container);
        }

        // Create nested scopes
        using var outerScope = container.CreateScope();
        using var innerScope = outerScope.ServiceProvider.CreateScope();
        
        var outerOptions = outerScope.ServiceProvider.GetRequiredService<IOptions<TestAsyncOptions>>().Value;
        var innerOptions = innerScope.ServiceProvider.GetRequiredService<IOptions<TestAsyncOptions>>().Value;

        // Assert - both scopes should resolve the same configured options
        Assert.NotNull(outerOptions);
        Assert.NotNull(innerOptions);
        Assert.Equal("AsyncConfigured", outerOptions.Value);
        Assert.Equal("AsyncConfigured", innerOptions.Value);
    }

    private static ShellBlueprint CreateBlueprint()
    {
        return new ShellBlueprint
        {
            Settings = new ShellSettings(),
            Descriptor = new ShellDescriptor(),
            Dependencies = new Dictionary<Type, IEnumerable<IFeatureInfo>>(),
        };
    }

    private static FeatureInfo AddStartup(ShellBlueprint shellBlueprint, Type startupType)
    {
        var featureInfo = new FeatureInfo(startupType.Name, startupType.Name, 1, "Tests", null, new ExtensionInfo(startupType.Name), null, false, false, false);
        shellBlueprint.Dependencies.Add(startupType, [featureInfo]);

        return featureInfo;
    }

    #region Test Options Classes

    private sealed class TestAsyncOptions
    {
        public string Value { get; set; }
    }

    private sealed class CountingOptions
    {
        public int ConfigurationCount { get; set; }
    }

    private sealed class MixedOptions
    {
        public string SyncConfiguredValue { get; set; }
        public string AsyncConfiguredValue { get; set; }
    }

    private sealed class OrderTrackingOptions
    {
        public List<string> ExecutionOrder { get; } = [];
    }

    #endregion

    #region Test Startups

    private sealed class AsyncOptionsStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureAsync<TestAsyncOptions>((sp, options) =>
            {
                options.Value = "AsyncConfigured";
                return ValueTask.CompletedTask;
            });
        }
    }

    private sealed class TrackingAsyncOptionsStartup : StartupBase
    {
        public static bool ConfigurationExecuted { get; set; }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureAsync<TestAsyncOptions>((sp, options) =>
            {
                ConfigurationExecuted = true;
                return ValueTask.CompletedTask;
            });
        }
    }

    private sealed class AsyncConfigureOptionsInterfaceStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureAsync<TestAsyncOptions, TestAsyncConfigureOptions>();
        }
    }

    private sealed class TestAsyncConfigureOptions : IAsyncConfigureOptions<TestAsyncOptions>
    {
        public static bool ConfigureAsyncCalled { get; set; }

        public ValueTask ConfigureAsync(TestAsyncOptions options)
        {
            ConfigureAsyncCalled = true;
            options.Value = "ConfiguredByInterface";
            return ValueTask.CompletedTask;
        }
    }

    private sealed class MultipleConfiguratorStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureAsync<CountingOptions>((sp, options) =>
            {
                options.ConfigurationCount++;
                return ValueTask.CompletedTask;
            });

            services.ConfigureAsync<CountingOptions>((sp, options) =>
            {
                options.ConfigurationCount++;
                return ValueTask.CompletedTask;
            });
        }
    }

    private sealed class MixedConfigurationStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Standard sync configuration
            services.Configure<MixedOptions>(options =>
            {
                options.SyncConfiguredValue = "SyncValue";
            });

            // Async configuration
            services.ConfigureAsync<MixedOptions>((sp, options) =>
            {
                options.AsyncConfiguredValue = "AsyncValue";
                return ValueTask.CompletedTask;
            });
        }
    }

    private sealed class ConfigureAndPostConfigureStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Register ConfigureAsync first
            services.ConfigureAsync<OrderTrackingOptions>((sp, options) =>
            {
                options.ExecutionOrder.Add("ConfigureAsync");
                return ValueTask.CompletedTask;
            });

            // Register PostConfigureAsync second
            services.PostConfigureAsync<OrderTrackingOptions>((sp, options) =>
            {
                options.ExecutionOrder.Add("PostConfigureAsync");
                return ValueTask.CompletedTask;
            });
        }
    }

    private sealed class PostAsyncConfigureOptionsInterfaceStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.PostConfigureAsync<TestAsyncOptions, TestPostAsyncConfigureOptions>();
        }
    }

    private sealed class TestPostAsyncConfigureOptions : IPostAsyncConfigureOptions<TestAsyncOptions>
    {
        public static bool PostConfigureAsyncCalled { get; set; }

        public ValueTask PostConfigureAsync(TestAsyncOptions options)
        {
            PostConfigureAsyncCalled = true;
            options.Value = "PostConfigured";
            return ValueTask.CompletedTask;
        }
    }

    #endregion
}
