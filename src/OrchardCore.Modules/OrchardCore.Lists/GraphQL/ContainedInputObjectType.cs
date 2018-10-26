using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedInputObjectType : InputObjectGraphType<ContainedPart>
    {
        public ContainedInputObjectType()
        {
            Name = "ContainedPartInput";

            Field("listContentItemId", x => x.ListContentItemId, true)
                .Type(new StringGraphType())
                .Description("the content item id of the parent list of the content item to filter")
                ;
        }
    }
}
