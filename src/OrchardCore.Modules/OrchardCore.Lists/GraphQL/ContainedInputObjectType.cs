using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedInputObjectType : WhereInputObjectGraphType<ContainedPart>
    {
        public ContainedInputObjectType()
        {
            Name = "ContainedPartInput";
            Description = "the custom URL part of the content item";

            AddScalarFilterFields<IdGraphType>("listContentItemId", "the content item id of the parent list of the content item to filter");
        }
    }
}
