using GraphQL.Types;
using OrchardCore.Autoroute.Model;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutorouteInputObjectType : InputObjectGraphType<AutoroutePart>
    {
        public AutorouteInputObjectType()
        {
            Name = "AutoroutePartInput";
            Description = "the custom URL part of the content item";

            Field("path", x => x.Path, nullable: true)
                .Type(new StringGraphType())
                .Description("the path of the content item to filter");
        }
    }
}
