using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.JsonApi;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;

namespace OrchardCore.Alias.JsonApi
{
    [RequireFeatures("OrchardCore.Apis.JsonApi")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IJsonApiResourceHandler<ContentItem>, AliasResourceHandler>();
        }
    }
}
