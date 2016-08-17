using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Theming;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment.Extensions;
using Orchard.Events;
using Orchard.Tests.Stubs;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;
using System.Threading.Tasks;

namespace Orchard.Tests.DisplayManagement
{
    public class DefaultDisplayManagerTests
    {
        ShapeTable _defaultShapeTable;
        IServiceProvider _serviceProvider;

        public class StubEventBus : IEventBus
        {
            public Task NotifyAsync(string message, IDictionary<string, object> arguments)
            {
                return null;
            }

            public Task NotifyAsync<TEventHandler>(Expression<Func<TEventHandler, Task>> eventNotifier) where TEventHandler : IEventHandler
            {
                return Task.CompletedTask;
            }

            public void Subscribe(string message, Func<IServiceProvider, IDictionary<string, object>, Task> action)
            {
            }
        }


        public DefaultDisplayManagerTests()
        {
            _defaultShapeTable = new ShapeTable
            {
                Descriptors = new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
                Bindings = new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
            };

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IThemeManager, ThemeManager>();
            serviceCollection.AddScoped<IHttpContextAccessor, StubHttpContextAccessor>();
            serviceCollection.AddScoped<IHtmlDisplay, DefaultIHtmlDisplay>();
            serviceCollection.AddScoped<IShapeTableManager, TestShapeTableManager>();
            serviceCollection.AddScoped<IShapeDisplayEvents, TestDisplayEvents>();
            serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
            serviceCollection.AddScoped<IEventBus, StubEventBus>();
            serviceCollection.AddLogging();

            serviceCollection.AddSingleton(_defaultShapeTable);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        class TestDisplayEvents : IShapeDisplayEvents
        {
            public Action<ShapeDisplayingContext> Displaying = ctx => { };
            public Action<ShapeDisplayedContext> Displayed = ctx => { };

            void IShapeDisplayEvents.Displaying(ShapeDisplayingContext context) { Displaying(context); }
            void IShapeDisplayEvents.Displayed(ShapeDisplayedContext context) { Displayed(context); }
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

            var shape = new Shape
            {
                Metadata = new ShapeMetadata
                {
                    Type = "Foo"
                }
            };

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

            var shape = new Shape
            {
                Metadata = new ShapeMetadata
                {
                    Type = "Foo"
                }
            };

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

            var shape = new Shape
            {
                Metadata = new ShapeMetadata
                {
                    Type = "Foo__2"
                }
            };

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

            var shape = new Shape
            {
                Metadata = new ShapeMetadata
                {
                    Type = "Foo__2"
                }
            };

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

            var shape = new Shape
            {
                Metadata = new ShapeMetadata
                {
                    Type = "Foo"
                }
            };
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


        //[Fact]
        //public void IShapeDisplayEventsIsCalled() {
        //    var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        //    var shape = new Shape {
        //        Metadata = new ShapeMetadata {
        //            Type = "Foo"
        //        }
        //    };

        //    var descriptor = new ShapeDescriptor {
        //        ShapeType = "Foo",
        //    };
        //    AddBinding(descriptor, "Foo", ctx => new HtmlString("yarg"));
        //    AddShapeDescriptor(descriptor);

        //    var displayingEventCount = 0;
        //    var displayedEventCount = 0;
        //    _container.Resolve<TestDisplayEvents>().Displaying = ctx => { ++displayingEventCount; };
        //    _container.Resolve<TestDisplayEvents>().Displayed = ctx => { ++displayedEventCount; ctx.ChildContent = new HtmlString("[" + ctx.ChildContent.ToHtmlString() + "]"); };

        //    var result = displayManager.Execute(CreateDisplayContext(shape));

        //    Assert.That(displayingEventCount, Is.EqualTo(1));
        //    Assert.That(displayedEventCount, Is.EqualTo(1));
        //    Assert.That(result.ToString(), Is.EqualTo("[yarg]"));
        //}


        [Fact]
        public async Task ShapeDescriptorDisplayingAndDisplayedAreCalled()
        {
            var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

            var shape = new Shape
            {
                Metadata = new ShapeMetadata
                {
                    Type = "Foo"
                }
            };

            var descriptor = new ShapeDescriptor
            {
                ShapeType = "Foo",
            };
            AddBinding(descriptor, "Foo", ctx => Task.FromResult<IHtmlContent>(new HtmlString("yarg")));
            AddShapeDescriptor(descriptor);

            var displayingEventCount = 0;
            var displayedEventCount = 0;
            descriptor.Displaying = new Action<ShapeDisplayingContext>[] { ctx => { ++displayingEventCount; } };
            descriptor.Displayed = new Action<ShapeDisplayedContext>[] { ctx => { ++displayedEventCount; ctx.ChildContent = new HtmlString("[" + ctx.ChildContent.ToString() + "]"); } };

            var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));

            Assert.Equal(1, displayingEventCount);
            Assert.Equal(1, displayedEventCount);
            Assert.Equal("[yarg]", result.ToString());
        }

        [Fact]
        public async Task DisplayingEventFiresEarlyEnoughToAddAlternateShapeBindingNames()
        {
            var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

            var shapeFoo = new Shape
            {
                Metadata = new ShapeMetadata
                {
                    Type = "Foo"
                }
            };
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


            var resultNormally = await displayManager.ExecuteAsync(CreateDisplayContext(shapeFoo));
            descriptorFoo.Displaying = new Action<ShapeDisplayingContext>[] { ctx => ctx.ShapeMetadata.Alternates.Add("Bar") };
            var resultWithOverride = await displayManager.ExecuteAsync(CreateDisplayContext(shapeFoo));

            Assert.Equal("alpha", resultNormally.ToString());
            Assert.Equal("beta", resultWithOverride.ToString());
        }


        [Fact]
        public async Task ShapeTypeAndBindingNamesAreNotCaseSensitive()
        {
            var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

            var shapeFoo = new Shape
            {
                Metadata = new ShapeMetadata
                {
                    Type = "foo"
                }
            };
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