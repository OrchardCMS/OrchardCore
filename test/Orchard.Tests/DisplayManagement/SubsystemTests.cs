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
using System.Collections.Generic;
using System.Threading.Tasks;
using Orchard.Environment.Extensions.Models;
using System.IO;
using Microsoft.Extensions.WebEncoders;

namespace Orchard.Tests.DisplayManagement
{
    public class SubsystemTests
    {
        IServiceProvider _serviceProvider;

        public class StubEventBus : IEventBus
        {
            public Task NotifyAsync(string message, IDictionary<string, object> arguments)
            {
                return null;
            }

            public Task NotifyAsync<TEventHandler>(Expression<Action<TEventHandler>> eventNotifier) where TEventHandler : IEventHandler
            {
                return null;
            }

            public void Subscribe(string message, Func<IServiceProvider, IDictionary<string, object>, Task> action)
            {
            }
        }

        public SubsystemTests()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            var testFeature = new Feature
            {
                Descriptor = new FeatureDescriptor
                {
                    Id = "Testing",
                    Extension = new ExtensionDescriptor
                    {
                        Id = "Testing",
                        ExtensionType = DefaultExtensionTypes.Module,
                    }
                }
            };

            serviceCollection.AddScoped<IShapeTableProvider, ShapeAttributeBindingStrategy>();
            serviceCollection.AddScoped<ILoggerFactory, StubLoggerFactory>();
            serviceCollection.AddScoped<IFeatureManager, StubFeatureManager>();
            serviceCollection.AddScoped<IDisplayManager, DefaultDisplayManager>();
            serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
            serviceCollection.AddScoped<IDisplayHelperFactory, DisplayHelperFactory>();
            serviceCollection.AddScoped<IEventBus, StubEventBus>();
            serviceCollection.AddScoped<IShapeTableManager, DefaultShapeTableManager>();

            serviceCollection.AddScoped<IHttpContextAccessor, StubHttpContextAccessor>();
            serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
            serviceCollection.AddInstance<ITypeFeatureProvider>(
                new TypeFeatureProvider(new Dictionary<Type, Feature>() {
                    { typeof(SimpleShapes), testFeature }
                }));

            serviceCollection.AddInstance(new SimpleShapes());

            new ShapeAttributeBindingModule().Configure(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public class SimpleShapes
        {
            [Shape]
            public IHtmlContent Something()
            {
                return new HtmlString("<br/>");
            }

            [Shape]
            public IHtmlContent Pager()
            {
                return new HtmlString("<div>hello</div>");
            }
        }

        [Fact]
        public void RenderingSomething()
        {
            dynamic displayHelperFactory = _serviceProvider.GetService<IDisplayHelperFactory>().CreateHelper(new ViewContext());
            dynamic shapeHelperFactory = _serviceProvider.GetService<IShapeFactory>();

            var result1 = displayHelperFactory.Something();
            var result2 = ((DisplayHelper)displayHelperFactory).ShapeExecute(shapeHelperFactory.Pager());
            var result3 = ((DisplayHelper)displayHelperFactory).ShapeExecute((Shape)shapeHelperFactory.Pager());

            displayHelperFactory(shapeHelperFactory.Pager());

            Assert.Equal("<br/>", HtmlContentUtilities.HtmlContentToString((IHtmlContent)result1));
            Assert.Equal("<div>hello</div>", HtmlContentUtilities.HtmlContentToString((IHtmlContent)result2));
            Assert.Equal("<div>hello</div>", HtmlContentUtilities.HtmlContentToString(result3));
        }
    }

    public class HtmlContentUtilities
    {
        public static string HtmlContentToString(IHtmlContent content, HtmlEncoder encoder = null)
        {
            if (encoder == null)
            {
                encoder = HtmlEncoder.Default;
            }

            using (var writer = new StringWriter())
            {
                content.WriteTo(writer, encoder);
                return writer.ToString();
            }
        }
    }
}