using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedInputObjectType : WhereInputObjectGraphType<ContainedPart>
    {
        public ContainedInputObjectType(IStringLocalizer<ContainedPart> T)
        {
            Name = "ContainedPartInput";
            Description = T["the list part of the content item"];

            AddScalarFilterFields<IdGraphType>("listContentItemId", T["the content item id of the parent list of the content item to filter"]);
        }
    }
}
