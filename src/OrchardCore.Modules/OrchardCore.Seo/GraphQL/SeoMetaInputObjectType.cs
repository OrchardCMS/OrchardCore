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

        Field(x => x.PageTitle, true)
            .Description("The seo page title");
    }
}
