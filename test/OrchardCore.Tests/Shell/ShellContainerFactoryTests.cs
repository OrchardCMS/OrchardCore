using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Tests.Stubs;
using StartupBase = OrchardCore.Modules.StartupBase;

namespace OrchardCore.Tests.Shell;

public class ShellContainerFactoryTests
{
    private static readonly ShellSettings _uninitializedDefaultShell = new ShellSettings()
        .AsDefaultShell()
        .AsUninitialized();

    private readonly ShellContainerFactory _shellContainerFactory;
    private readonly IServiceProvider _applicationServiceProvider;

    public ShellContainerFactoryTests()
    {
        var applicationServices = new ServiceCollection();
        applicationServices.AddSingleton<ITypeFeatureProvider, TypeFeatureProvider>();

        applicationServices.AddSingleton<ITestSingleton, TestSingleton>();
        applicationServices.AddTransient<ITestTransient, TestTransient>();
        applicationServices.AddScoped<ITestScoped, TestScoped>();

        applicationServices.AddSingleton<ITwoHostSingletonsOfTheSameType, FirstHostSingletonsOfTheSameType>();
        applicationServices.AddSingleton<ITwoHostSingletonsOfTheSameType, SecondHostSingletonsOfTheSameType>();

        applicationServices.AddSingleton<IHostSingletonAndScopedOfTheSameType, HostSingletonOfTheSameTypeAsScoped>();
        applicationServices.AddScoped<IHostSingletonAndScopedOfTheSameType, HostScopedOfTheSameTypeAsSingleton>();

        _shellContainerFactory = new ShellContainerFactory(
            new StubHostingEnvironment(),
            new StubExtensionManager(),
            _applicationServiceProvider = applicationServices.BuildServiceProvider(),
            applicationServices
        );
    }

    [Fact]
    public async Task CanRegisterDefaultServiceWithFeatureInfo()
    {
        var shellBlueprint = CreateBlueprint();

        var expectedFeatureInfo = AddStartup(shellBlueprint, typeof(RegisterServiceStartup));

        var container = (await _shellContainerFactory
            .CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint))
            .CreateScope()
            .ServiceProvider;

        var typeFeatureProvider = _applicationServiceProvider.GetService<ITypeFeatureProvider>();

        Assert.IsType<TestService>(container.GetRequiredService(typeof(ITestService)));
        Assert.Same(expectedFeatureInfo, typeFeatureProvider.GetFeatureForDependency(typeof(TestService)));
    }

    [Fact]
    public async Task CanReplaceDefaultServiceWithCustomService()
    {
        var shellBlueprint = CreateBlueprint();

        var expectedFeatureInfo = AddStartup(shellBlueprint, typeof(ReplaceServiceStartup));
        AddStartup(shellBlueprint, typeof(RegisterServiceStartup));

        var container = (await _shellContainerFactory
            .CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint))
            .CreateScope()
            .ServiceProvider;

        var typeFeatureProvider = _applicationServiceProvider.GetService<ITypeFeatureProvider>();

        // Check that the default service has been replaced with the custom service and that the feature info is correct.
        Assert.IsType<CustomTestService>(container.GetRequiredService(typeof(ITestService)));
        Assert.Same(expectedFeatureInfo, typeFeatureProvider.GetFeatureForDependency(typeof(CustomTestService)));
    }

    [Fact]
    public async Task HostServiceLifeTimesShouldBePreserved()
    {
        var shellBlueprint = CreateBlueprint();
        var container = (await _shellContainerFactory
            .CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint))
            .CreateScope()
            .ServiceProvider;

        var appSingleton = _applicationServiceProvider.GetRequiredService<ITestSingleton>();

        var singleton1 = container.GetRequiredService<ITestSingleton>();
        var singleton2 = container.GetRequiredService<ITestSingleton>();
        var transient1 = container.GetRequiredService<ITestTransient>();
        var transient2 = container.GetRequiredService<ITestTransient>();
        var scoped1 = container.GetRequiredService<ITestScoped>();
        var scoped2 = container.GetRequiredService<ITestScoped>();

        ITestScoped scoped3, scoped4;
        using (var scope = container.CreateScope())
        {
            scoped3 = scope.ServiceProvider.GetRequiredService<ITestScoped>();
            scoped4 = scope.ServiceProvider.GetRequiredService<ITestScoped>();
        }

        Assert.IsType<TestSingleton>(singleton1);
        Assert.IsType<TestTransient>(transient1);
        Assert.IsType<TestTransient>(transient2);
        Assert.IsType<TestScoped>(scoped1);
        Assert.IsType<TestScoped>(scoped3);

        Assert.Equal(singleton1, singleton2);
        Assert.Same(appSingleton, singleton1);
        Assert.NotEqual(transient1, transient2);
        Assert.NotEqual(scoped1, scoped3);
        Assert.Equal(scoped1, scoped2);
        Assert.Equal(scoped3, scoped4);
    }

    [Fact]
    public async Task WhenTwoHostSingletons_GetServices_Returns_HostAndShellServices()
    {
        var shellBlueprint = CreateBlueprint();
        AddStartup(shellBlueprint, typeof(ServicesOfTheSameTypeStartup));

        var container = (await _shellContainerFactory
            .CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint))
            .CreateScope()
            .ServiceProvider;

        var services = container.GetServices<ITwoHostSingletonsOfTheSameType>();

        Assert.Equal(5, services.Count());
    }

    [Fact]
    public async Task WhenHostSingletonAndScoped_GetServices_Returns_CorrectImplementations()
    {
        var shellBlueprint = CreateBlueprint();

        var container = (await _shellContainerFactory
            .CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint))
            .CreateScope()
            .ServiceProvider;

        var services = container.GetServices<IHostSingletonAndScopedOfTheSameType>();

        Assert.Equal(2, services.Count());
        Assert.IsType<HostSingletonOfTheSameTypeAsScoped>(services.ElementAt(0));
        Assert.IsType<HostScopedOfTheSameTypeAsSingleton>(services.ElementAt(1));
    }

    [Fact]
    public async Task AssignsTypeToMultipleFeatures()
    {
        var shellBlueprint = CreateBlueprint();

        var expectedFeatureInfos = AddStartups(shellBlueprint, typeof(RegisterServiceStartup), typeof(RegisterSecondServiceStartup));

        var container = (await _shellContainerFactory
            .CreateContainerAsync(_uninitializedDefaultShell, shellBlueprint))
            .CreateScope()
            .ServiceProvider;

        var typeFeatureProvider = _applicationServiceProvider.GetService<ITypeFeatureProvider>();

        Assert.IsType<TestService>(container.GetRequiredService(typeof(ITestService)));
        Assert.Equal(expectedFeatureInfos, typeFeatureProvider.GetFeaturesForDependency(typeof(TestService)));
    }

    private static ShellBlueprint CreateBlueprint()
    {
        return new ShellBlueprint
        {
            Settings = new ShellSettings(),
            Descriptor = new ShellDescriptor(),
            Dependencies = new Dictionary<Type, IEnumerable<IFeatureInfo>>()
        };
    }

    public static IFeatureInfo AddStartup(ShellBlueprint shellBlueprint, Type startupType)
    {
        var featureInfo = new FeatureInfo(startupType.Name, startupType.Name, 1, "Tests", null, new ExtensionInfo(startupType.Name), null, false, false, false);
        shellBlueprint.Dependencies.Add(startupType, [featureInfo]);

        return featureInfo;
    }

    public static IFeatureInfo[] AddStartups(ShellBlueprint shellBlueprint, Type startupType1, Type startupType2)
    {
        var featureInfo1 = new FeatureInfo(startupType1.Name, startupType1.Name, 1, "Tests", null, new ExtensionInfo(startupType1.Name), null, false, false, false);
        var featureInfo2 = new FeatureInfo(startupType2.Name, startupType2.Name, 1, "Tests", null, new ExtensionInfo(startupType2.Name), null, false, false, false);
        shellBlueprint.Dependencies.Add(startupType1, [featureInfo1]);
        shellBlueprint.Dependencies.Add(startupType2, [featureInfo2]);

        return [featureInfo1, featureInfo2];
    }

    private interface ITestService
    {
    }

    private sealed class TestService : ITestService
    {
    }

    private sealed class CustomTestService : ITestService
    {
    }

    private sealed class RegisterServiceStartup : StartupBase
    {
        public override int Order => 1;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ITestService, TestService>();
        }
    }

    private sealed class RegisterSecondServiceStartup : StartupBase
    {
        public override int Order => 1;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ITestService, TestService>();
        }
    }

    private sealed class ReplaceServiceStartup : StartupBase
    {
        public override int Order => 2;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Scoped(typeof(ITestService), typeof(CustomTestService)));
        }
    }

    private interface ITestSingleton { }

    private interface ITestTransient { }

    private interface ITestScoped { }

    private sealed class TestSingleton : ITestSingleton { }

    private sealed class TestTransient : ITestTransient { }

    private sealed class TestScoped : ITestScoped { }

    private interface ITwoHostSingletonsOfTheSameType { }

    private sealed class FirstHostSingletonsOfTheSameType : ITwoHostSingletonsOfTheSameType { }

    private sealed class SecondHostSingletonsOfTheSameType : ITwoHostSingletonsOfTheSameType { }

    private sealed class ShellSingletonOfTheSametype : ITwoHostSingletonsOfTheSameType { }

    private sealed class ShellTransientOfTheSametype : ITwoHostSingletonsOfTheSameType { }

    private sealed class ShellScopedOfTheSametype : ITwoHostSingletonsOfTheSameType { }

    private interface IHostSingletonAndScopedOfTheSameType { }

    private sealed class HostSingletonOfTheSameTypeAsScoped : IHostSingletonAndScopedOfTheSameType { }

    private sealed class HostScopedOfTheSameTypeAsSingleton : IHostSingletonAndScopedOfTheSameType { }

    private sealed class ServicesOfTheSameTypeStartup : StartupBase
    {
        public override int Order => 1;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITwoHostSingletonsOfTheSameType, ShellSingletonOfTheSametype>();
            services.AddTransient<ITwoHostSingletonsOfTheSameType, ShellTransientOfTheSametype>();
            services.AddScoped<ITwoHostSingletonsOfTheSameType, ShellScopedOfTheSametype>();
        }
    }
}
