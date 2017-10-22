using GraphQL.Types;
using OrchardCore.Flows.Models;

namespace OrchardCore.Apis.GraphQL.Types
{
    public class BagPartType : ObjectGraphType<BagPart>
    {
        public BagPartType()
        {
            Name = typeof(BagPart).Name;

            // TODO : BagPart should represent a `connection`
            // https://facebook.github.io/relay/docs/graphql-connections.html
            Field<ListGraphType<ContentItemType>>("ContentItems", resolve: 
                context => context.Source.ContentItems
            );

            Interface<ContentPartInterface>();
            
            IsTypeOf = value => value is BagPart;
        }
    }
}
