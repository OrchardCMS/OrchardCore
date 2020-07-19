using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;

namespace OrchardCore.Media.Shape
{
    [RequireFeatures("OrchardCore.Media")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShapeTableProvider, MediaFieldShapes>();
        }
    }
}
