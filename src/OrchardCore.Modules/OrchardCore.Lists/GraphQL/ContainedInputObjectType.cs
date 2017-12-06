using GraphQL.Types;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedInputObjectType : InputObjectGraphType<ContainedPart>
    {
        public ContainedInputObjectType()
        {
            Name = "ContainedPartInput";

            Field("ParentContentItemId", x => x.ListContentItemId, false);
            Field(x => x.Order, false);
        }
    }
}
