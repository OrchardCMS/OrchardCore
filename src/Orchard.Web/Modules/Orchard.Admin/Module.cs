using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;
using Orchard.Routes;

namespace Orchard.Admin
{
    public class Module : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IRouteProvider, Routes>();
            serviceCollection.AddScoped<IFilterMetadata, AdminFilter>();
            serviceCollection.AddScoped<IFilterMetadata, AdminMenuFilter>();
        }
    }
}
