using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Modules;

namespace OrchardCore.ContentLocalization.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddInputObjectGraphType<LocalizationPart, LocalizationInputObjectType>();
            services.AddObjectGraphType<LocalizationPart, LocalizationQueryObjectType>();
            services.AddTransient<IIndexAliasProvider, LocalizationPartIndexAliasProvider>();
            services.AddWhereInputIndexPropertyProvider<LocalizedContentItemIndex>();
        }
    }
}
