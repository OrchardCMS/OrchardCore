using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Tests.Stubs;
using StartupBase = OrchardCore.Modules.StartupBase;

namespace OrchardCore.Tests.Shell
{
    public class ShellContainerFactoryTests
    {
        private static readonly ShellSettings _uninitializedDefaultShell = new ShellSettings()
            .AsDefaultShell()
            .AsUninitialized();

        private readonly IShellContainerFactory _shellContainerFactory;
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

        private static ShellBlueprint CreateBlueprint()
        {
            return new ShellBlueprint
            {
                Settings = new ShellSettings(),
                Descriptor = new ShellDescriptor(),
                Dependencies = new Dictionary<Type, FeatureEntry>()
            };
        }

        public static IFeatureInfo AddStartup(ShellBlueprint shellBlueprint, Type startupType)
        {
            var featureInfo = new FeatureInfo(startupType.Name, startupType.Name, 1, "Tests", null, null, null, false, false, false);
            shellBlueprint.Dependencies.Add(startupType, new FeatureEntry(featureInfo));

            return featureInfo;
        }

        private interface ITestService
        {
        }

        private class TestService : ITestService
        {
        }

        private class CustomTestService : ITestService
        {
        }

        private class RegisterServiceStartup : StartupBase
        {
            public override int Order => 1;

            public override void ConfigureServices(IServiceCollection services)
            {
                services.AddScoped<ITestService, TestService>();
            }
        }

        private class ReplaceServiceStartup : StartupBase
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

        private class TestSingleton : ITestSingleton { }

        private class TestTransient : ITestTransient { }

        private class TestScoped : ITestScoped { }

        private interface ITwoHostSingletonsOfTheSameType { }

        private class FirstHostSingletonsOfTheSameType : ITwoHostSingletonsOfTheSameType { }

        private class SecondHostSingletonsOfTheSameType : ITwoHostSingletonsOfTheSameType { }

        private class ShellSingletonOfTheSametype : ITwoHostSingletonsOfTheSameType { }

        private class ShellTransientOfTheSametype : ITwoHostSingletonsOfTheSameType { }

        private class ShellScopedOfTheSametype : ITwoHostSingletonsOfTheSameType { }

        private interface IHostSingletonAndScopedOfTheSameType { }

        private class HostSingletonOfTheSameTypeAsScoped : IHostSingletonAndScopedOfTheSameType { }

        private class HostScopedOfTheSameTypeAsSingleton : IHostSingletonAndScopedOfTheSameType { }

        private class ServicesOfTheSameTypeStartup : StartupBase
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
}
