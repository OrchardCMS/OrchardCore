using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedInputObjectType : QueryArgumentObjectGraphType<ContainedPart>
    {
        public ContainedInputObjectType()
        {
            Name = "ContainedPartInput";

            AddInputField("listContentItemId", x => x.ListContentItemId, true);
            AddInputField("order", x => x.Order, true);
        }
    }
}
