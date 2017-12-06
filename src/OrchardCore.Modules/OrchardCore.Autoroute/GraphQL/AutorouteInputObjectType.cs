using GraphQL.Types;
using OrchardCore.Autoroute.Model;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutorouteInputObjectType : InputObjectGraphType<AutoroutePart>
    {
        public AutorouteInputObjectType()
        {
            Name = "AutoroutePartInput";

            this.AddInputField("path", x => x.Path, true);
            this.AddInputField("setHomepage", x => x.SetHomepage, true);
        }
    }
}
