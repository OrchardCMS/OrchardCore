using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Apis.JsonApi
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddEnumerable(
               ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, MvcJsonApiMvcOptionsSetup>());
        }
    }
}
