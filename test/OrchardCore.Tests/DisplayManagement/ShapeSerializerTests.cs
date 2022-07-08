using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;
using OrchardCore.Tests.Stubs;
using Xunit;

namespace OrchardCore.Tests.DisplayManagement
{
    public class ShapeSerializerTests
    {
        private IServiceProvider _serviceProvider;

        public ShapeSerializerTests()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging();
            serviceCollection.AddScoped<IHtmlDisplay, DefaultHtmlDisplay>();
            serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
            serviceCollection.AddScoped<IThemeManager, ThemeManager>();
            serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
            serviceCollection.AddScoped<IShapeTableManager, TestShapeTableManager>();

            var defaultShapeTable = new ShapeTable
            (
                new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
            );

            serviceCollection.AddSingleton(defaultShapeTable);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async Task ShouldSerialize()
        {
            var shape = _serviceProvider.GetService<IShapeFactory>();

            var alpha = await shape.CreateAsync("Alpha");
            var serialized = alpha.ShapeToJson();

            Assert.Contains("Alpha", serialized.ToString());
        }

        [Fact]
        public async Task ShouldSkipRecursiveShapes()
        {
            var shape = _serviceProvider.GetService<IShapeFactory>();

            var alpha = await shape.CreateAsync("Alpha");

            var beta = await shape.CreateAsync("Beta", Arguments.From(new
            {
                Alpha = alpha
            }));

            await alpha.AddAsync(beta);
            var serialized = alpha.ShapeToJson();

            Assert.Contains("Beta", serialized.ToString());
        }
    }
}
