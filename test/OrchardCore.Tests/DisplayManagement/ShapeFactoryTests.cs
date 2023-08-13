using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.DisplayManagement
{
    public class ShapeFactoryTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ShapeTable _shapeTable;

        public ShapeFactoryTests()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<ILoggerFactory, NullLoggerFactory>();
            serviceCollection.AddScoped<IThemeManager, ThemeManager>();
            serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
            serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
            serviceCollection.AddScoped<IShapeTableManager, TestShapeTableManager>();

            _shapeTable = new ShapeTable
            (
                new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
            );

            serviceCollection.AddSingleton(_shapeTable);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async Task ShapeHasAttributesType()
        {
            var factory = _serviceProvider.GetService<IShapeFactory>();
            dynamic foo = await factory.CreateAsync("Foo", ArgsUtility.Empty());
            ShapeMetadata metadata = foo.Metadata;
            Assert.Equal("Foo", metadata.Type);
        }

        [Fact]
        public async Task CreateShapeWithNamedArguments()
        {
            var factory = _serviceProvider.GetService<IShapeFactory>();
            dynamic foo = await factory.CreateAsync("Foo", ArgsUtility.Named(new { one = 1, two = "dos" }));
            Assert.Equal(1, foo.one);
            Assert.Equal("dos", foo.two);
        }

        [Fact]
        public async Task CallSyntax()
        {
            dynamic factory = _serviceProvider.GetService<IShapeFactory>();
            var foo = await factory.Foo();
            ShapeMetadata metadata = foo.Metadata;
            Assert.Equal("Foo", metadata.Type);
        }

        [Fact]
        public async Task CallInitializer()
        {
            dynamic factory = _serviceProvider.GetService<IShapeFactory>();
            var bar = new { One = 1, Two = "two" };
            var foo = await factory.Foo(bar);

            Assert.Equal(1, foo.One);
            Assert.Equal("two", foo.Two);
        }

        [Fact]
        public async Task ShapeFactoryUsesCustomShapeType()
        {
            var descriptor = new ShapeDescriptor
            {
                CreatingAsync = new List<Func<ShapeCreatingContext, Task>>()
                {
                    (ctx) =>
                    {
                        ctx.Create = () => new SubShape();
                        return Task.CompletedTask;
                    },
                },
            };

            _shapeTable.Descriptors.Add("Foo", descriptor);
            dynamic factory = _serviceProvider.GetService<IShapeFactory>();
            var foo = await factory.Foo();

            Assert.IsType<SubShape>(foo);
        }

        [Fact]
        public async Task ShapeFactoryWithCustomShapeTypeAppliesArguments()
        {
            var descriptor = new ShapeDescriptor
            {
                CreatingAsync = new List<Func<ShapeCreatingContext, Task>>()
                {
                    (ctx) =>
                    {
                        ctx.Create = () => new SubShape();
                        return Task.CompletedTask;
                    },
                },
            };

            _shapeTable.Descriptors.Add("Foo", descriptor);
            dynamic factory = _serviceProvider.GetService<IShapeFactory>();
            var foo = await factory.Foo(Bar: "Bar", Baz: "Baz");

            Assert.Equal("Bar", foo.Bar);
            Assert.Equal("Baz", foo.Baz);
        }

        private class SubShape : Shape
        {
        }
    }
}
