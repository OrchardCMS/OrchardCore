using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.GraphQL
{
    public class FlowPartQueryObjectType : ObjectGraphType<FlowPart>
    {
        public FlowPartQueryObjectType()
        {
            Name = "FlowPart";

            Field<ListGraphType<ContentItemInterface>>(
                "widgets",
                "The widgets.",
                resolve: context => context.Source.Widgets);
        }
    }
}
