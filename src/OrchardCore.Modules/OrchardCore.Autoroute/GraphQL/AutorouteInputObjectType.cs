using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Autoroute.Model;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutorouteInputObjectType : QueryArgumentObjectGraphType<AutoroutePart>
    {
        public AutorouteInputObjectType()
        {
            Name = "AutoroutePartInput";
            
            AddInputField("path", x => x.Path, true);
            AddInputField("setHomepage", x => x.SetHomepage, true);
        }
    }
}
