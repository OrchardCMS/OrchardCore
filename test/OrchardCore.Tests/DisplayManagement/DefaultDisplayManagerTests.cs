using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;
using OrchardCore.Tests.Stubs;
using Xunit;

namespace OrchardCore.Tests.DisplayManagement
{
    public class DefaultDisplayManagerTests
    {
        TestShapeTable _defaultShapeTable;
        IServiceProvider _serviceProvider;

        public DefaultDisplayManagerTests()
        {
            _defaultShapeTable = new TestShapeTable
            {
                Descriptors = new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
                Bindings = new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
            };

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IThemeManager, ThemeManager>();
            serviceCollection.AddScoped<IHttpContextAccessor, StubHttpContextAccessor>();
            serviceCollection.AddScoped<IHtmlDisplay, DefaultHtmlDisplay>();
            serviceCollection.AddScoped<IShapeTableManager, TestShapeTableManager>();
            serviceCollection.AddScoped<IShapeDisplayEvents, TestDisplayEvents>();
            serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
            serviceCollection.AddScoped<IStringLocalizer<DefaultHtmlDisplay>, NullStringLocalizer<DefaultHtmlDisplay>>();
            serviceCollection.AddLogging();

            serviceCollection.AddSingleton(_defaultShapeTable);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        class TestDisplayEvents : IShapeDisplayEvents
        {
            public Action<ShapeDisplayContext> Displaying = ctx => { };
            public Action<ShapeDisplayContext> Displayed = ctx => { };
            public Action<ShapeDisplayContext> Finalized = ctx => { };

            Task IShapeDisplayEvents.DisplayingAsync(ShapeDisplayContext context) { Displaying(context); return Task.CompletedTask; }
            Task IShapeDisplayEvents.DisplayedAsync(ShapeDisplayContext context) { Displayed(context); return Task.CompletedTask; }
            Task IShapeDisplayEvents.DisplayingFinalizedAsync(ShapeDisplayContext context) { Finalized(context); return Task.CompletedTask; }
        }

        void AddShapeDescriptor(ShapeDescriptor shapeDescriptor)
        {
            _defaultShapeTable.Descriptors[shapeDescriptor.ShapeType] = shapeDescriptor;
            foreach (var binding in shapeDescriptor.Bindings)
            {
                binding.Value.ShapeDescriptor = shapeDescriptor;
                _defaultShapeTable.Bindings[binding.Key] = binding.Value;
            }
        }

        static DisplayContext CreateDisplayContext(Shape shape)
        {
            return new DisplayContext
            {
                Value = shape,
                ViewContext = new ViewContext()
            };
        }

        [Fact]
        public async Task RenderSimpleShape()
        {
            var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

            var shape = new Shape();
            shape.Metadata.Type = "Foo";

            var descriptor = new ShapeDescriptor
            {
                ShapeType = "Foo",
            };
            descriptor.Bindings["Foo"] = new ShapeBinding
            {
                BindingName = "Foo",
                BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi there!")),
            };
            AddShapeDescriptor(descriptor);

            var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
            Assert.Equal("Hi there!", result.ToString());
        }

        [Fact]
        public async Task RenderPreCalculatedShape()
        {
            var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

            var shape = new Shape();
            shape.Metadata.Type = "Foo";

            shape.Metadata.OnDisplaying(
                context =>
                {
                    context.ChildContent = new HtmlString("Bar");
                });

            var descriptor = new ShapeDescriptor
            {
                ShapeType = "Foo",
            };
            descriptor.Bindings["Foo"] = new ShapeBinding
            {
                BindingName = "Foo",
                BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi there!")),
            };

            AddShapeDescriptor(descriptor);

            var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
            Assert.Equal("Bar", result.ToString());
        }

        [Fact]
        public async Task RenderFallbackShape()
        {
            var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

            var shape = new Shape();
            shape.Metadata.Type = "Foo__2";

            var descriptor = new ShapeDescriptor
            {
                ShapeType = "Foo",
            };
            descriptor.Bindings["Foo"] = new ShapeBinding
            {
                BindingName = "Foo",
                BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi there!")),
            };
            AddShapeDescriptor(descriptor);

            var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
            Assert.Equal("Hi there!", result.ToString());
        }

        [Fact]
        public async Task RenderAlternateShapeExplicitly()
        {
            var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

            var shape = new Shape();
            shape.Metadata.Type = "Foo__2";

            var descriptor = new ShapeDescriptor
            {
                ShapeType = "Foo",
            };
            descriptor.Bindings["Foo"] = new ShapeBinding
            {
                BindingName = "Foo",
                BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi there!")),
            };
            descriptor.Bindings["Foo__2"] = new ShapeBinding
            {
                BindingName = "Foo__2",
                BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hello again!")),
            };
            AddShapeDescriptor(descriptor);

            var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
            Assert.Equal("Hello again!", result.ToString());
        }

        [Fact]
        public async Task RenderAlternateShapeByMostRecentlyAddedMatchingAlternate()
        {
            var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

            var shape = new Shape();
            shape.Metadata.Type = "Foo";

            shape.Metadata.Alternates.Add("Foo__1");
            shape.Metadata.Alternates.Add("Foo__2");
            shape.Metadata.Alternates.Add("Foo__3");

            var descriptor = new ShapeDescriptor
            {
                ShapeType = "Foo",
            };
            AddBinding(descriptor, "Foo", ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi there!")));
            AddBinding(descriptor, "Foo__1", ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hello (1)!")));
            AddBinding(descriptor, "Foo__2", ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hello (2)!")));
            AddShapeDescriptor(descriptor);

            var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
            Assert.Equal("Hello (2)!", result.ToString());
        }

        private static void AddBinding(ShapeDescriptor descriptor, string bindingName, Func<DisplayContext, Task<IHtmlContent>> binding)
        {
            descriptor.Bindings[bindingName] = new ShapeBinding
            {
                BindingName = bindingName,
                BindingAsync = binding,
            };
        }

        [Fact]
        public async Task ShapeDescriptorDisplayingAndDisplayedAreCalled()
        {
            var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

            var shape = new Shape();
            shape.Metadata.Type = "Foo";

            var descriptor = new ShapeDescriptor
            {
                ShapeType = "Foo",
            };
            AddBinding(descriptor, "Foo", ctx => Task.FromResult<IHtmlContent>(new HtmlString("yarg")));
            AddShapeDescriptor(descriptor);

            var displayingEventCount = 0;
            var displayedEventCount = 0;
            descriptor.DisplayingAsync = new Func<ShapeDisplayContext, Task>[] { ctx => { ++displayingEventCount; return Task.CompletedTask; } };
            descriptor.DisplayedAsync = new Func<ShapeDisplayContext, Task>[] { ctx => { ++displayedEventCount; ctx.ChildContent = new HtmlString("[" + ctx.ChildContent.ToString() + "]"); return Task.CompletedTask; } };

            var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));

            Assert.Equal(1, displayingEventCount);
            Assert.Equal(1, displayedEventCount);
            Assert.Equal("[yarg]", result.ToString());
        }

        [Fact]
        public async Task DisplayingEventFiresEarlyEnoughToAddAlternateShapeBindingNames()
        {
            var htmlDisplay = _serviceProvider.GetService<IHtmlDisplay>();


            var shapeFoo = new Shape();
            shapeFoo.Metadata.Type = "Foo";

            var descriptorFoo = new ShapeDescriptor
            {
                ShapeType = "Foo",
            };
            AddBinding(descriptorFoo, "Foo", ctx => Task.FromResult<IHtmlContent>(new HtmlString("alpha")));
            AddShapeDescriptor(descriptorFoo);

            var descriptorBar = new ShapeDescriptor
            {
                ShapeType = "Bar",
            };
            AddBinding(descriptorBar, "Bar", ctx => Task.FromResult<IHtmlContent>(new HtmlString("beta")));
            AddShapeDescriptor(descriptorBar);


            var resultNormally = await htmlDisplay.ExecuteAsync(CreateDisplayContext(shapeFoo));

            shapeFoo = new Shape();
            shapeFoo.Metadata.Type = "Foo";
            descriptorFoo.DisplayingAsync = new Func<ShapeDisplayContext, Task>[] { ctx => { ctx.ShapeMetadata.Alternates.Add("Bar"); return Task.CompletedTask; } };
            var resultWithOverride = await htmlDisplay.ExecuteAsync(CreateDisplayContext(shapeFoo));

            Assert.Equal("alpha", resultNormally.ToString());
            Assert.Equal("beta", resultWithOverride.ToString());
        }


        [Fact]
        public async Task ShapeTypeAndBindingNamesAreNotCaseSensitive()
        {
            var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

            var shapeFoo = new Shape();
            shapeFoo.Metadata.Type = "Foo";

            var descriptorFoo = new ShapeDescriptor
            {
                ShapeType = "Foo",
            };
            AddBinding(descriptorFoo, "Foo", ctx => Task.FromResult<IHtmlContent>(new HtmlString("alpha")));
            AddShapeDescriptor(descriptorFoo);

            var result = await displayManager.ExecuteAsync(CreateDisplayContext(shapeFoo));

            Assert.Equal("alpha", result.ToString());
        }
    }
}