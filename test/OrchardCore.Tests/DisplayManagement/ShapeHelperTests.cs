using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.DisplayManagement
{
    public class ShapeHelperTests
    {
        private readonly IServiceProvider _serviceProvider;

        public ShapeHelperTests()
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
        public async Task CreatingNewShapeTypeByName()
        {
            dynamic shape = _serviceProvider.GetService<IShapeFactory>();

            var alpha = await shape.Alpha();

            Assert.Equal("Alpha", alpha.Metadata.Type);
        }

        [Fact]
        public async Task CreatingShapeWithAdditionalNamedParameters()
        {
            dynamic shape = _serviceProvider.GetService<IShapeFactory>();

            var alpha = await shape.Alpha(one: 1, two: "dos");

            Assert.Equal("Alpha", alpha.Metadata.Type);
            Assert.Equal(1, alpha.one);
            Assert.Equal("dos", alpha.two);
        }

        [Fact]
        public async Task WithPropertyBearingObjectInsteadOfNamedParameters()
        {
            dynamic shape = _serviceProvider.GetService<IShapeFactory>();

            var alpha = await shape.Alpha(new { one = 1, two = "dos" });

            Assert.Equal("Alpha", alpha.Metadata.Type);
            Assert.Equal(1, alpha.one);
            Assert.Equal("dos", alpha.two);
        }
    }
}
