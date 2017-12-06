using GraphQL.Types;
using OrchardCore.Autoroute.Model;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutorouteInputObjectType : InputObjectGraphType<AutoroutePart>
    {
        public AutorouteInputObjectType()
        {
            Name = "AutoroutePartInput";

            Field(x => x.Path, false);
            Field(x => x.SetHomepage, false);
        }
    }
}
