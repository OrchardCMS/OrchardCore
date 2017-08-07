using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.ContentFields.Media
{
    [RequireFeatures("Orchard.Media")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShapeTableProvider, MediaShapes>();
        }
    }
}
