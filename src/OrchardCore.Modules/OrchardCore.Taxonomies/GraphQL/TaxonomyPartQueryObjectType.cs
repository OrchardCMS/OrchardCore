using System.Collections.Generic;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Taxonomies.GraphQL
{
    public class TaxonomyPartQueryObjectType : ObjectGraphType<TaxonomyPart>
    {
        public TaxonomyPartQueryObjectType()
        {
            Name = "TaxonomyPart";

            Field(x => x.TermContentType);

            Field<ListGraphType<ContentItemInterface>, IEnumerable<ContentItem>>()
                .Name("contentItems")
                .Description("the content items")
                .PagingArguments()
                .Resolve(x => x.Page(x.Source.Terms));
        }
    }
}
