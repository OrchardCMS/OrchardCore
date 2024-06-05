using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.GraphQL.Fields;
using OrchardCore.ContentFields.GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public sealed class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IContentFieldProvider, ObjectGraphTypeFieldProvider>();
            services.AddTransient<IContentFieldProvider, ContentFieldsProvider>();

            services.AddObjectGraphType<LinkField, LinkFieldQueryObjectType>();
            services.AddObjectGraphType<HtmlField, HtmlFieldQueryObjectType>();
            services.AddObjectGraphType<ContentPickerField, ContentPickerFieldQueryObjectType>();
            services.AddObjectGraphType<UserPickerField, UserPickerFieldQueryObjectType>();
        }
    }
}
