using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Autoroute.Model;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutorouteInputObjectType : InputObjectGraphType<AutoroutePart>, IQueryArgumentObjectGraphType
    {
        public AutorouteInputObjectType()
        {
            Name = "AutoroutePartInput";

            this.AddInputField("path", x => x.Path, true);
            this.AddInputField("setHomepage", x => x.SetHomepage, true);
        }
    }
}
