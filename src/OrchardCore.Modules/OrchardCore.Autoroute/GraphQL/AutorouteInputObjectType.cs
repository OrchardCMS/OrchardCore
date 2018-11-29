using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Autoroute.Model;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutorouteInputObjectType : WhereInputObjectGraphType<AutoroutePart>
    {
        public AutorouteInputObjectType(IStringLocalizer<AutorouteInputObjectType> T)
        {
            Name = "AutoroutePartInput";
            Description = T["the custom URL part of the content item"];

            AddScalarFilterFields<StringGraphType>("path", T["the path of the content item to filter"]);
        }
    }
}
