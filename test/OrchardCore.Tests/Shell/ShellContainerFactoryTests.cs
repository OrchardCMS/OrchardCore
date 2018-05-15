using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using OrchardCore.Tests.Stubs;
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

            _shellContainerFactory = new ShellContainerFactory(
                new StubHostingEnvironment(),
                new StubExtensionManager(),
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
    }
}