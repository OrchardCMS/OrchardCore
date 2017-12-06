using GraphQL.Types;
using OrchardCore.Autoroute.Model;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutorouteQueryObjectType : ObjectGraphType<AutoroutePart>
    {
        public AutorouteQueryObjectType()
        {
            Name = "AutoroutePart";

            Field(x => x.Path);
        }
    }
}
