using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Extensions.Features.Attributes;
using OrchardCore.Environment.Shell;
using Xunit;

namespace OrchardCore.Tests.Shell
{
    public class DependencyLoaderTests
    {
        private readonly IDependencyLoader _dependencyLoader;

        public DependencyLoaderTests()
        {
            _dependencyLoader = new DependencyLoader();
        }

        [Fact]
        public void RegistersTypesWithServiceImplAttribute()
        {
            var serviceCollection = new ServiceCollection();
            var blueprintTypes = new[] { typeof(IDiscoverableService), typeof(INonDiscoverableService), typeof(DiscoverableService), typeof(NonDiscoverableService) };

            _dependencyLoader.RegisterDependencies(blueprintTypes, serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var discoverableService = serviceProvider.GetService<IDiscoverableService>();

            Assert.NotNull(discoverableService);
            Assert.IsType<DiscoverableService>(discoverableService);
        }

        [Fact]
        public void DoesNotRegisterTypesWithoutServiceImplAttribute()
        {
            var serviceCollection = new ServiceCollection();
            var blueprintTypes = new[] { typeof(IDiscoverableService), typeof(INonDiscoverableService), typeof(DiscoverableService), typeof(NonDiscoverableService) };

            _dependencyLoader.RegisterDependencies(blueprintTypes, serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var nonDiscoverableService = serviceProvider.GetService<INonDiscoverableService>();

            Assert.Null(nonDiscoverableService);
        }

        [Fact]
        public void CanOverrideServicesUsingOverrideServiceAttribute()
        {
            var serviceCollection = new ServiceCollection();
            var blueprintTypes = new[] { typeof(IDiscoverableService), typeof(DiscoverableService), typeof(OverrideDiscoverableService) };

            _dependencyLoader.RegisterDependencies(blueprintTypes, serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var overridingDiscoverableService = serviceProvider.GetService<IDiscoverableService>();

            Assert.IsType<OverrideDiscoverableService>(overridingDiscoverableService);
        }

        [Fact]
        public void RegistersSelfSufficientTypes()
        {
            var serviceCollection = new ServiceCollection();
            var blueprintTypes = new[] { typeof(SelfService) };

            _dependencyLoader.RegisterDependencies(blueprintTypes, serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var selfService = serviceProvider.GetService<SelfService>();

            Assert.NotNull(selfService);
        }

        [Fact]
        public void RegistersTypesWithMultipleServices()
        {
            var serviceCollection = new ServiceCollection();
            var blueprintTypes = new[] { typeof(MultipleServices), typeof(IMultipleServices) };

            _dependencyLoader.RegisterDependencies(blueprintTypes, serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service1 = serviceProvider.GetService<MultipleServices>();
            var service2 = serviceProvider.GetService<IMultipleServices>();

            Assert.NotNull(service1);
            Assert.NotNull(service2);
            Assert.IsType<MultipleServices>(service2);
        }

        [Fact]
        public void RegistersTypesWithExternalServices()
        {
            var serviceCollection = new ServiceCollection();
            var blueprintTypes = new[] { typeof(ImplementsExternalService) };

            _dependencyLoader.RegisterDependencies(blueprintTypes, serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<Microsoft.AspNetCore.Html.IHtmlContent>();

            Assert.NotNull(service);
            Assert.IsType<ImplementsExternalService>(service);
        }

        public static IEnumerable<Type> GetTypesFromBlueprint()
        {
            return new[]
            {
                typeof(IDiscoverableService),
                typeof(INonDiscoverableService),
                typeof(DiscoverableService),
                typeof(NonDiscoverableService),
                typeof(OverrideDiscoverableService)
            };
        }

        [Service]
        private interface IDiscoverableService
        {
        }

        private interface INonDiscoverableService
        {
        }

        [ServiceImpl]
        private class DiscoverableService : IDiscoverableService
        {
        }

        private class NonDiscoverableService : INonDiscoverableService
        {
        }

        [ServiceOverride(typeof(DiscoverableService))]
        private class OverrideDiscoverableService : IDiscoverableService
        {
        }

        [ServiceImpl, Service]
        private class SelfService
        {
        }

        [Service(ServiceLifetime.Singleton)]
        private interface IMultipleServices
        {
        }

        [ServiceImpl, Service(ServiceLifetime.Singleton)]
        private class MultipleServices : IMultipleServices
        {
        }

        [ServiceImpl, Service(typeof(Microsoft.AspNetCore.Html.IHtmlContent))]
        private class ImplementsExternalService : Microsoft.AspNetCore.Html.IHtmlContent
        {
            public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            {
                throw new NotImplementedException();
            }
        }
    }
}