using Microsoft.Extensions.DependencyInjection;
using Orchard.Recipes;
using Orchard.Themes.Recipes;

namespace Orchard.Themes
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddRecipeExecutionStep<ThemesStep>();
        }
    }
}
