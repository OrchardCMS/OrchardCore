using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.DependencyInjection;
using Orchard.Routes;

namespace Orchard.Contents
{
    public class Module : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddContentManagement();
            serviceCollection.AddContentManagementDisplay();
            serviceCollection.AddScoped<IRouteProvider, Routes>();
        }
    }
}