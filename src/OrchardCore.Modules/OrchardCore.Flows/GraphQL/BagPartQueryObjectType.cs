using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.GraphQL
{
    public class BagPartQueryObjectType : ObjectGraphType<BagPart>
    {
        public BagPartQueryObjectType()
        {
            Name = "BagPart";

            Field<ListGraphType<ContentItemInterface>>(
                "items",
                "The items.",
                resolve: context => context.Source.ContentItems);
        }
    }
}
