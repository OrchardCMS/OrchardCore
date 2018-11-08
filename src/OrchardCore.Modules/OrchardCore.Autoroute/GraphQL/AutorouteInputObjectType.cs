using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Autoroute.Model;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutorouteInputObjectType : WhereInputObjectGraphType<AutoroutePart>
    {
        public AutorouteInputObjectType()
        {
            Name = "AutoroutePartInput";
            Description = "the custom URL part of the content item";

            AddScalarFilterFields<StringGraphType>("path", "the path of the content item to filter");
        }
    }
}
