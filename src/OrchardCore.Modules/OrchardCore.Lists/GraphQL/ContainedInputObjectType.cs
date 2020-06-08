using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedInputObjectType : WhereInputObjectGraphType<ContainedPart>
    {
        public ContainedInputObjectType(IStringLocalizer<ContainedPart> S)
        {
            Name = "ContainedPartInput";
            Description = S["the list part of the content item"];

            AddScalarFilterFields<IdGraphType>("listContentItemId", S["the content item id of the parent list of the content item to filter"]);
        }
    }
}
