using System;
using Orchard.Environment.Shell;
using System.Collections.Generic;
using Xunit;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.Environment.Shell.Builders;
using Orchard.Environment.Shell.Builders.Models;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions;

namespace Orchard.Tests.Shell
{
    public class ShellContainerFactoryTests
    {
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        private readonly IShellContainerFactory _shellContainerFactory;

        public ShellContainerFactoryTests()
        {
            var applicationServices = new ServiceCollection();
            _typeFeatureProvider = new TypeFeatureProvider();
            applicationServices.AddScoped(x => _typeFeatureProvider);

            _shellContainerFactory = new ShellContainerFactory(
                new ServiceCollection().BuildServiceProvider(),
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

            var container = _shellContainerFactory.CreateContainer(ShellHelper.BuildDefaultUninitializedShell, shellBlueprint);

            Assert.IsType<TestService>(container.GetRequiredService(typeof(ITestService)));
            Assert.Same(expectedFeatureInfo, _typeFeatureProvider.GetFeatureForDependency(typeof(TestService)));
        }

        [Fact]
        public void CanReplaceDefaultServiceWithCustomService()
        {
            var shellBlueprint = CreateBlueprint();

            var expectedFeatureInfo = AddStartup(shellBlueprint, typeof(ReplaceServiceStartup));
            AddStartup(shellBlueprint, typeof(RegisterServiceStartup));

            var container = _shellContainerFactory.CreateContainer(ShellHelper.BuildDefaultUninitializedShell, shellBlueprint);

            // Check that the default service has been replaced with the custom service and that the feature info is correct.
            Assert.IsType<CustomTestService>(container.GetRequiredService(typeof(ITestService)));
            Assert.Same(expectedFeatureInfo, _typeFeatureProvider.GetFeatureForDependency(typeof(CustomTestService)));
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