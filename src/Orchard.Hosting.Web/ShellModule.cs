using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Data;
using Orchard.DependencyInjection;
using Orchard.Environment.Shell;
using Orchard.Hosting.Mvc.Routing;
using Orchard.Hosting.ProcessingQueue;
using Orchard.Hosting.Routing.Routes;
using Orchard.Hosting.Web.Routing.Routes;
using Orchard.Processing;
using Orchard.Routes;

namespace Orchard.Hosting
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class ShellModule : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDataAccess();

            serviceCollection.AddScoped<IOrchardShellEvents, OrchardShell>();
            serviceCollection.AddSingleton<IRunningShellRouterTable, DefaultRunningShellRouterTable>();

            serviceCollection.AddSingleton<IRouteBuilder, DefaultShellRouteBuilder>();
            serviceCollection.AddSingleton<IRoutePublisher, RoutePublisher>();

            serviceCollection.AddScoped<IDeferredTaskEngine, Orchard.Processing.DefaultDeferredTaskEngine>();
            serviceCollection.AddScoped<IDeferredTaskState, HttpContextTaskState>();
        }
    }
}