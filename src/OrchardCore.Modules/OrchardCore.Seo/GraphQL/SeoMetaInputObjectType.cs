using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Seo.Models;

namespace OrchardCore.Seo.GraphQL;

public class SeoMetaInputObjectType : WhereInputObjectGraphType<SeoMetaPart>
{
    public SeoMetaInputObjectType(IStringLocalizer<SeoMetaInputObjectType> S)
    {
        Name = "SeoMetaPartInput";
        Description = S["SEO meta fields"];

        Field(x => x.Render, true)
            .Description("Whether to render the seo metas");
        Field(x => x.PageTitle, true)
            .Description("The seo page title");
        Field(x => x.MetaDescription, true)
            .Description("The meta description of the content item");
        Field(x => x.MetaKeywords, true)
            .Description("The meta keywords of the content item");
        Field(x => x.Canonical, true)
            .Description("The canonical link of the content item");
        Field( x => x.MetaRobots, true)
            .Description("The content item specific meta robots definition");
    }
}
