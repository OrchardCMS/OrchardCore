using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Extensions;
using Orchard.Tests.Stubs;
using Xunit;

namespace Orchard.Tests.DisplayManagement
{
    public class ShapeFactoryTests
    {
        IServiceProvider _serviceProvider;
        TestShapeTable _shapeTable;

        public ShapeFactoryTests()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<ILoggerFactory, StubLoggerFactory>();
            serviceCollection.AddScoped<IThemeManager, ThemeManager>();
            serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
            serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
            serviceCollection.AddScoped<IShapeTableManager, TestShapeTableManager>();

            _shapeTable = new TestShapeTable
            {
                Descriptors = new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
                Bindings = new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
            };

            serviceCollection.AddSingleton(_shapeTable);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public void ShapeHasAttributesType()
        {
            var factory = _serviceProvider.GetService<IShapeFactory>();
            dynamic foo = factory.Create("Foo", ArgsUtility.Empty());
            ShapeMetadata metadata = foo.Metadata;
            Assert.Equal("Foo", metadata.Type);
        }

        [Fact]
        public void CreateShapeWithNamedArguments()
        {
            var factory = _serviceProvider.GetService<IShapeFactory>();
            dynamic foo = factory.Create("Foo", ArgsUtility.Named(new { one = 1, two = "dos" }));
            Assert.Equal(1, foo.one);
            Assert.Equal("dos", foo.two);
        }

        [Fact]
        public void CallSyntax()
        {
            dynamic factory = _serviceProvider.GetService<IShapeFactory>();
            var foo = factory.Foo();
            ShapeMetadata metadata = foo.Metadata;
            Assert.Equal("Foo", metadata.Type);
        }

        [Fact]
        public void CallInitializer()
        {
            dynamic factory = _serviceProvider.GetService<IShapeFactory>();
            var bar = new { One = 1, Two = "two" };
            var foo = factory.Foo(bar);

            Assert.Equal(1, foo.One);
            Assert.Equal("two", foo.Two);
        }

        [Fact]
        public void ShapeFactoryUsesCustomShapeType()
        {
            var descriptor = new ShapeDescriptor();
            descriptor.Creating = new List<Action<ShapeCreatingContext>>()
            {
                (ctx) => { ctx.Create = () => new SubShape(); }
            };

            _shapeTable.Descriptors.Add("Foo", descriptor);
            dynamic factory = _serviceProvider.GetService<IShapeFactory>();
            var foo = factory.Foo();

            Assert.IsType<SubShape>(foo);
        }

        [Fact]
        public void ShapeFactoryWithCustomShapeTypeAppliesArguments()
        {
            var descriptor = new ShapeDescriptor();
            descriptor.Creating = new List<Action<ShapeCreatingContext>>()
            {
                (ctx) => { ctx.Create = () => new SubShape(); }
            };

            _shapeTable.Descriptors.Add("Foo", descriptor);
            dynamic factory = _serviceProvider.GetService<IShapeFactory>();
            var foo = factory.Foo(Bar: "Bar", Baz: "Baz");

            Assert.Equal("Bar", foo.Bar);
            Assert.Equal("Baz", foo.Baz);
        }


        private class SubShape : Shape
        {

        }
    }
}