using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedQueryObjectType : ObjectGraphType<ContainedPart>
    {
        public ContainedQueryObjectType(IStringLocalizer<ContainedQueryObjectType> T)
        {
            Name = "ContainedPart";
            Description = T["Represents a link to the parent content item, and the order that content item is represented."];

            Field(x => x.ListContentItemId);
            Field(x => x.Order);
        }
    }
}
