using GraphQL.Types;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedInputObjectType : InputObjectGraphType<ContainedPart>
    {
        public ContainedInputObjectType()
        {
            Name = "ContainedPartInput";

            this.AddInputField("listContentItemId", x => x.ListContentItemId, true);
            this.AddInputField("order", x => x.Order, true);
        }
    }
}
