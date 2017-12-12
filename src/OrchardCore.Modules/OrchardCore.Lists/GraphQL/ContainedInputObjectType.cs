using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedInputObjectType : InputObjectGraphType<ContainedPart>, IQueryArgumentObjectGraphType
    {
        public ContainedInputObjectType()
        {
            Name = "ContainedPartInput";

            this.AddInputField("listContentItemId", x => x.ListContentItemId, true);
            this.AddInputField("order", x => x.Order, true);
        }
    }
}
