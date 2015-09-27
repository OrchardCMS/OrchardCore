using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.DependencyInjection;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment.Extensions;
using Orchard.Tests.Stubs;
using System;
using Xunit;

namespace Orchard.Tests.DisplayManagement {
    public class SubsystemTests {
        IServiceProvider _serviceProvider;

        public SubsystemTests(){
            IServiceCollection serviceCollection = new ServiceCollection();
            
            serviceCollection.AddScoped<IShapeTableLocator, ShapeTableLocator>();
            serviceCollection.AddScoped<IDisplayManager, DefaultDisplayManager>();
            serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
            serviceCollection.AddScoped<IDisplayHelperFactory, DisplayHelperFactory>();
            serviceCollection.AddScoped<IShapeTableManager, DefaultShapeTableManager>();

            serviceCollection.AddScoped<IHttpContextAccessor, StubHttpContextAccessor>();
            serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Fact(Skip = "Not Working")]
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
}
