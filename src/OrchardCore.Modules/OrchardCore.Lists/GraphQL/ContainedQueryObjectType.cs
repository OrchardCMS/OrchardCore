using GraphQL.Types;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedQueryObjectType : ObjectGraphType<ContainedPart>
    {
        public ContainedQueryObjectType()
        {
            Name = "ContainedPart";

            Field(x => x.ListContentItemId);
            Field(x => x.Order);
        }
    }
}
