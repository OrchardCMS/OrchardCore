//using Microsoft.AspNetCore.Html;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Orchard.DisplayManagement;
//using Orchard.DisplayManagement.Descriptors;
//using Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy;
//using Orchard.DisplayManagement.Implementation;
//using Orchard.DisplayManagement.Shapes;
//using Orchard.DisplayManagement.Theming;
//using Orchard.Environment.Extensions;
//using Orchard.Environment.Extensions.Features;
//using Orchard.Events;
//using Orchard.Tests.Stubs;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq.Expressions;
//using System.Text.Encodings.Web;
//using System.Threading.Tasks;
//using Xunit;

//namespace Orchard.Tests.DisplayManagement
//{
//    public class SubsystemTests
//    {
//        IServiceProvider _serviceProvider;

//        public class StubEventBus : IEventBus
//        {
//            public Task NotifyAsync(string message, IDictionary<string, object> arguments)
//            {
//                return null;
//            }

//            public Task NotifyAsync<TEventHandler>(Expression<Func<TEventHandler, Task>> eventNotifier) where TEventHandler : IEventHandler
//            {
//                return null;
//            }

//            public void Subscribe(string message, Func<IServiceProvider, IDictionary<string, object>, Task> action)
//            {
//            }
//        }

//        public SubsystemTests()
//        {
//            IServiceCollection serviceCollection = new ServiceCollection();

//            var testFeature = new Feature
//            {
//                Descriptor = new FeatureDescriptor
//                {
//                    Id = "Testing",
//                    Extension = new ExtensionDescriptor
//                    {
//                        Id = "Testing",
//                        ExtensionType = DefaultExtensionTypes.Module,
//                    }
//                }
//            };

//            serviceCollection.AddScoped<IShapeTableProvider, ShapeAttributeBindingStrategy>();
//            serviceCollection.AddScoped<ILogger<DefaultShapeTableManager>, NullLogger<DefaultShapeTableManager>>();
//            serviceCollection.AddScoped<ILogger<DefaultIHtmlDisplay>, NullLogger<DefaultIHtmlDisplay>>();
//            serviceCollection.AddScoped<IFeatureManager, StubFeatureManager>();
//            serviceCollection.AddScoped<IHtmlDisplay, DefaultIHtmlDisplay>();
//            serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
//            serviceCollection.AddScoped<IDisplayHelperFactory, DisplayHelperFactory>();
//            serviceCollection.AddScoped<IEventBus, StubEventBus>();
//            serviceCollection.AddScoped<IShapeTableManager, DefaultShapeTableManager>();

//            serviceCollection.AddScoped<IHttpContextAccessor, StubHttpContextAccessor>();
//            serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
//            serviceCollection.AddSingleton<ITypeFeatureProvider, TypeFeatureProvider>();
//            serviceCollection.AddSingleton<IThemeManager>(x =>
//                new MockThemeManager(testFeature.Descriptor.Extension)
            
//            );

//            serviceCollection.AddSingleton<IMemoryCache>(x => new MemoryCache(new MemoryCacheOptions()));

//            serviceCollection.AddSingleton(new SimpleShapes());

//            _serviceProvider = serviceCollection.BuildServiceProvider();
//        }

//        public class SimpleShapes
//        {
//            [Shape]
//            public IHtmlContent Something()
//            {
//                return new HtmlString("<br/>");
//            }

//            [Shape]
//            public IHtmlContent Pager()
//            {
//                return new HtmlString("<div>hello</div>");
//            }
//        }

//        [Fact(Skip = "This test (or the underlying code) must be reworked.")]
//        public async Task RenderingSomething()
//        {
//            dynamic displayHelperFactory = _serviceProvider.GetService<IDisplayHelperFactory>().CreateHelper(new ViewContext());
//            dynamic shapeHelperFactory = _serviceProvider.GetService<IShapeFactory>();

//            var result1 = displayHelperFactory.Something();
//            var result2 = await ((DisplayHelper)displayHelperFactory).ShapeExecuteAsync(shapeHelperFactory.Pager());
//            var result3 = await ((DisplayHelper)displayHelperFactory).ShapeExecuteAsync((Shape)shapeHelperFactory.Pager());

//            displayHelperFactory(shapeHelperFactory.Pager());

//            Assert.Equal("<br/>", HtmlContentUtilities.HtmlContentToString((IHtmlContent)result1));
//            Assert.Equal("<div>hello</div>", HtmlContentUtilities.HtmlContentToString((IHtmlContent)result2));
//            Assert.Equal("<div>hello</div>", HtmlContentUtilities.HtmlContentToString(result3));
//        }
//    }

//    public class HtmlContentUtilities
//    {
//        public static string HtmlContentToString(IHtmlContent content, HtmlEncoder encoder = null)
//        {
//            if (encoder == null)
//            {
//                encoder = HtmlEncoder.Default;
//            }

//            using (var writer = new StringWriter())
//            {
//                content.WriteTo(writer, encoder);
//                return writer.ToString();
//            }
//        }
//    }
//}