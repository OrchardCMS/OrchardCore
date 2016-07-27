using Microsoft.Extensions.DependencyInjection;
using Orchard.BackgroundTasks;
using Orchard.Data;
using Orchard.DeferredTasks;
using Orchard.ResourceManagement;

namespace Orchard.Hosting
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class ShellModule : StartupBase
    {
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDeferredTasks();
            serviceCollection.AddDataAccess();
            serviceCollection.AddResourceManagement();
            serviceCollection.AddBackgroundTasks();
        }
    }
}