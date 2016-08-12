using Microsoft.Extensions.DependencyInjection;
using Orchard.Modules.Recipes.Executors;
using Orchard.Recipes.Services;

namespace Orchard.Modules
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Module : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRecipeExecutionStep, FeatureStep>();
        }
    }
}
