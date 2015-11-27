using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment.Extensions;
using Orchard.Tests.Stubs;
using System;
using System.Collections.Generic;
using Xunit;

namespace Orchard.Tests.DisplayManagement
{
    public class ShapeFactoryTests
    {
        IServiceProvider _serviceProvider;

        public ShapeFactoryTests()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<ILoggerFactory, StubLoggerFactory>();
            serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
            serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
            serviceCollection.AddScoped<IShapeTableManager, TestShapeTableManager>();

            var defaultShapeTable = new ShapeTable
            {
                Descriptors = new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
                Bindings = new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
            };
            serviceCollection.AddInstance(defaultShapeTable);

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
        public void CallInitializerWithBaseType()
        {
            dynamic factory = _serviceProvider.GetService<IShapeFactory>();
            var bar = new { One = 1, Two = "two" };
            var foo = factory.Foo(typeof(MyShape), bar);

            Assert.IsType(typeof(MyShape), foo);
            Assert.Equal(1, foo.One);
            Assert.Equal("two", foo.Two);
        }

        public class MyShape : Shape
        {
            public string Kind { get; set; }
        }
    }
}