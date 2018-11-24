using System.Collections.Generic;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.GraphQL
{
    public class BagPartQueryObjectType : ObjectGraphType<BagPart>
    {
        public BagPartQueryObjectType()
        {
            Name = "BagPart";

            Field<ListGraphType<ContentItemInterface>, IEnumerable<ContentItem>>()
                .Name("contentItems")
                .Description("the content items")
                .PagingArguments()
                .Resolve(x => x.Page(x.Source.ContentItems));
        }
    }
}
