using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Autoroute.Models;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutorouteInputObjectType : WhereInputObjectGraphType<AutoroutePart>
    {
        public AutorouteInputObjectType(IStringLocalizer<AutorouteInputObjectType> S)
        {
            Name = "AutoroutePartInput";
            Description = S["the custom URL part of the content item"];

            AddScalarFilterFields<StringGraphType>("path", S["the path of the content item to filter"]);
        }
    }
}
