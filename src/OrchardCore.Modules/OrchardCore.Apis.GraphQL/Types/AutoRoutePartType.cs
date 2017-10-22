using GraphQL.Types;
using OrchardCore.Autoroute.Model;

namespace OrchardCore.Apis.GraphQL.Types
{
    public class AutoRoutePartType : AutoRegisteringObjectGraphType<AutoroutePart>
    {
        public AutoRoutePartType()
        {
            Name = typeof(AutoroutePart).Name;

            Interface<ContentPartInterface>();

            IsTypeOf = value => value is AutoroutePart;
        }
    }
}
