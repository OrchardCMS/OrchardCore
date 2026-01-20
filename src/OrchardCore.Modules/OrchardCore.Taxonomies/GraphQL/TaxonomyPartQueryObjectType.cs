using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Taxonomies.GraphQL;

public class TaxonomyPartQueryObjectType : ObjectGraphType<TaxonomyPart>
{
    public TaxonomyPartQueryObjectType(IStringLocalizer<TaxonomyPartQueryObjectType> S)
    {
        Name = "TaxonomyPart";

        Field(x => x.TermContentType);

        Field<ListGraphType<ContentItemInterface>, IEnumerable<ContentItem>>("contentItems")
            .Description(S["the content items"])
            .PagingArguments()
            .Resolve(x => x.Page(x.Source.Terms));
    }
}
