using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment.Extensions;
using Orchard.Events;
using Orchard.Tests.Stubs;
using System;
using Xunit;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Html.Abstractions;
using Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy;
using Orchard.Environment.Extensions.Features;

namespace Orchard.Tests.DisplayManagement {
    public class SubsystemTests {
        IServiceProvider _serviceProvider;

        public SubsystemTests(){
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IShapeTableProvider, ShapeAttributeBindingStrategy>();
            serviceCollection.AddScoped<ILoggerFactory, StubLoggerFactory>();
            serviceCollection.AddScoped<IFeatureManager, StubFeatureManager>();
            serviceCollection.AddScoped<IShapeTableLocator, ShapeTableLocator>();
            serviceCollection.AddScoped<IDisplayManager, DefaultDisplayManager>();
            serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
            serviceCollection.AddScoped<IDisplayHelperFactory, DisplayHelperFactory>();
            serviceCollection.AddScoped<IShapeTableManager, DefaultShapeTableManager>();

            serviceCollection.AddScoped<IHttpContextAccessor, StubHttpContextAccessor>();
            serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
            serviceCollection.AddScoped<IEventNotifier, StubEventNotifier>();

            serviceCollection.AddInstance(new SimpleShapes());

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public class SimpleShapes {
            [Shape]
            public IHtmlContent Something() {
                return new HtmlString("<br/>");
            }

            [Shape]
            public IHtmlContent Pager() {
                return new HtmlString("<div>hello</div>");
            }
        }

        [Fact]
        public void RenderingSomething() {
            dynamic displayHelperFactory = _serviceProvider.GetService<IDisplayHelperFactory>().CreateHelper(new ViewContext());
            dynamic shapeHelperFactory = _serviceProvider.GetService<IShapeFactory>();

            var result1 = displayHelperFactory.Something();
            var result2 = ((DisplayHelper)displayHelperFactory).ShapeExecute(shapeHelperFactory.Pager());
            var result3 = ((DisplayHelper)displayHelperFactory).ShapeExecute((Shape)shapeHelperFactory.Pager());

            displayHelperFactory(shapeHelperFactory.Pager());

            Assert.Equal("<br/>", result1.ToString());
            Assert.Equal("<div>hello</div>", result2.ToString());
            Assert.Equal("<div>hello</div>", result3.ToString());
        }
    }

    public class StubEventNotifier : IEventNotifier {
        public object Notify<TEventHandler>(Expression<Action<TEventHandler>> eventNotifier) where TEventHandler : IEventHandler {
            return null;
        }
    }
}
