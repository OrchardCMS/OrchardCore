using Microsoft.Extensions.DependencyInjection;
using Orchard.Data;
using Orchard.DeferredTasks;

namespace Orchard.Hosting
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class ShellModule : StartupBase
    {
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IDeferredTaskEngine, DeferredTaskEngine>();
            serviceCollection.AddScoped<IDeferredTaskState, HttpContextTaskState>();

            serviceCollection.AddDataAccess();
        }
    }
}