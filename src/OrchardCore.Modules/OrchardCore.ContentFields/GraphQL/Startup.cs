using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQLInputType<TextField, TextFieldInputObjectType>();
            services.AddGraphQLQueryType<TextField, TextFieldQueryObjectType>();
        }
    }
}
