using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Data.Migration;
using Orchard.Templates.Services;
using YesSql.Indexes;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.Templates
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShapeTableProvider, TemplatesBindingStrategy>();
        }
    }
}
