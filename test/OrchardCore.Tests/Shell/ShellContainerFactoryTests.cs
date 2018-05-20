using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using Xunit;

namespace OrchardCore.Tests.Shell
{
    public class ShellContainerFactoryTests
    {
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly IServiceProvider _applicationServiceProvider;

        public ShellContainerFactoryTests()
        {
            var applicationServices = new ServiceCollection();
            applicationServices.AddSingleton<ITypeFeatureProvider, TypeFeatureProvider>();

            applicationServices.AddSingleton<ITestSingleton, TestSingleton>();
            applicationServices.AddTransient<ITestTransient, TestTransient>();
            applicationServices.AddScoped<ITestScoped, TestScoped>();

            applicationServices.AddSingleton<ITwoHostSingletonsOfTheSameType, TwoHostSingletonsOfTheSameType1>();
            applicationServices.AddSingleton<ITwoHostSingletonsOfTheSameType, TwoHostSingletonsOfTheSameType2>();

            applicationServices.AddSingleton<IHostSingletonAndScopedOfTheSameType, HostSingletonAndScopedOfTheSameType1>();
            applicationServices.AddScoped<IHostSingletonAndScopedOfTheSameType, HostSingletonAndScopedOfTheSameType2>();

            _shellContainerFactory = new ShellContainerFactory(
                _applicationServiceProvider = applicationServices.BuildServiceProvider(),
                new StubLoggerFactory(),
                new NullLogger<ShellContainerFactory>(),
                applicationServices
            );
        }

        [Fact]
        public void CanRegisterDefaultServiceWithFeatureInfo()
        {
            var shellBlueprint = CreateBlueprint();

            var expectedFeatureInfo = AddStartup(shellBlueprint, typeof(RegisterServiceStartup));

            var container = _shellContainerFactory.CreateContainer(ShellHelper.BuildDefaultUninitializedShell, shellBlueprint).CreateScope().ServiceProvider;
            var typeFeatureProvider = _applicationServiceProvider.GetService<ITypeFeatureProvider>();

            Assert.IsType<TestService>(container.GetRequiredService(typeof(ITestService)));
            Assert.Same(expectedFeatureInfo, typeFeatureProvider.GetFeatureForDependency(typeof(TestService)));
        }

        [Fact]
        public void CanReplaceDefaultServiceWithCustomService()
        {
            var shellBlueprint = CreateBlueprint();

            var expectedFeatureInfo = AddStartup(shellBlueprint, typeof(ReplaceServiceStartup));
            AddStartup(shellBlueprint, typeof(RegisterServiceStartup));

            var container = _shellContainerFactory.CreateContainer(ShellHelper.BuildDefaultUninitializedShell, shellBlueprint).CreateScope().ServiceProvider;
            var typeFeatureProvider = _applicationServiceProvider.GetService<ITypeFeatureProvider>();

            // Check that the default service has been replaced with the custom service and that the feature info is correct.
            Assert.IsType<CustomTestService>(container.GetRequiredService(typeof(ITestService)));
            Assert.Same(expectedFeatureInfo, typeFeatureProvider.GetFeatureForDependency(typeof(CustomTestService)));
        }

        [Fact]
        public void HostServiceLifeTimesShouldBePreserved()
        {
            var shellBlueprint = CreateBlueprint();
            var container = _shellContainerFactory.CreateContainer(ShellHelper.BuildDefaultUninitializedShell, shellBlueprint).CreateScope().ServiceProvider;

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
        public void ResolvingIEnumerable_TwoHostSingletonsShouldNotHideShellServices()
        {
            var shellBlueprint = CreateBlueprint();
            AddStartup(shellBlueprint, typeof(EnumerableServiceStartup));
            var container = _shellContainerFactory.CreateContainer(ShellHelper.BuildDefaultUninitializedShell, shellBlueprint).CreateScope().ServiceProvider;

            var services = container.GetService<IEnumerable<ITwoHostSingletonsOfTheSameType>>();

            Assert.Equal(5, services.Count());
        }

        [Fact]
        public void ResolvingIEnumerable_HostSingletonAndScopedShouldNotInterfere()
        {
            var shellBlueprint = CreateBlueprint();
            var container = _shellContainerFactory.CreateContainer(ShellHelper.BuildDefaultUninitializedShell, shellBlueprint).CreateScope().ServiceProvider;

            var services = container.GetService<IEnumerable<IHostSingletonAndScopedOfTheSameType>>();

            Assert.Equal(2, services.Count());
            Assert.IsType<HostSingletonAndScopedOfTheSameType1>(services.ElementAt(0));
            Assert.IsType<HostSingletonAndScopedOfTheSameType2>(services.ElementAt(1));
        }

        private ShellBlueprint CreateBlueprint()
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
            var featureInfo = new FeatureInfo(startupType.Name, startupType.Name, 1, "Tests", null, null, null);
            shellBlueprint.Dependencies.Add(startupType, new NonCompiledFeatureEntry(featureInfo));

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
        private class TwoHostSingletonsOfTheSameType1 : ITwoHostSingletonsOfTheSameType { }
        private class TwoHostSingletonsOfTheSameType2 : ITwoHostSingletonsOfTheSameType { }
        private class TwoHostSingletonsOfTheSameType3 : ITwoHostSingletonsOfTheSameType { }
        private class TwoHostSingletonsOfTheSameType4 : ITwoHostSingletonsOfTheSameType { }
        private class TwoHostSingletonsOfTheSameType5 : ITwoHostSingletonsOfTheSameType { }

        private interface IHostSingletonAndScopedOfTheSameType { }
        private class HostSingletonAndScopedOfTheSameType1 : IHostSingletonAndScopedOfTheSameType { }
        private class HostSingletonAndScopedOfTheSameType2 : IHostSingletonAndScopedOfTheSameType { }

        private class EnumerableServiceStartup : StartupBase
        {
            public override int Order => 1;

            public override void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<ITwoHostSingletonsOfTheSameType, TwoHostSingletonsOfTheSameType3>();
                services.AddTransient<ITwoHostSingletonsOfTheSameType, TwoHostSingletonsOfTheSameType4>();
                services.AddScoped<ITwoHostSingletonsOfTheSameType, TwoHostSingletonsOfTheSameType5>();
            }
        }
    }
}