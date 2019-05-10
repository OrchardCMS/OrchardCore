using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Autoroute.Model;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutorouteQueryObjectType : ObjectGraphType<AutoroutePart>
    {
        public AutorouteQueryObjectType(IStringLocalizer<AutorouteQueryObjectType> T)
        {
            Name = "AutoroutePart";
            Description = T["Custom URLs (permalinks) for your content item."];

            Field(x => x.Path).Description(T["The permalinks for your content item."]);
        }
    }
}
