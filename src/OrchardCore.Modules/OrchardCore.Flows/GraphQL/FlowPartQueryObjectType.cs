using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.GraphQL
{
    public class FlowPartQueryObjectType : ObjectGraphType<FlowPart>
    {
        public FlowPartQueryObjectType(IStringLocalizer<FlowPartQueryObjectType> S)
        {
            Name = "FlowPart";
            Description = S["A FlowPart allows to add content items directly within another content item"];

            Field<ListGraphType<ContentItemInterface>>(
                "widgets",
                "The widgets.",
                resolve: context => context.Source.Widgets);
        }
    }
}
