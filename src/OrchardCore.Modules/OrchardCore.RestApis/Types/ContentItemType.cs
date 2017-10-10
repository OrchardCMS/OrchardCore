using GraphQL.Types;
using OrchardCore.Autoroute.Model;
using OrchardCore.ContentManagement;
using OrchardCore.Flows.Models;
using OrchardCore.Title.Model;

namespace OrchardCore.RestApis.Types
{
    public class ContentItemType : AutoRegisteringObjectGraphType<ContentItem>
    {
        public ContentItemType()
        {
            Name = "contentitem";
        }
    }

    public class ContentPartInterface : InterfaceGraphType<ContentElement>
    {
        public ContentPartInterface()
        {
            Name = "contentpart";
        }
    }

    public class TitlePartType : AutoRegisteringObjectGraphType<TitlePart>
    {
        public TitlePartType()
        {
            Name = "titlepart";

            Interface<ContentPartInterface>();

            IsTypeOf = value => value is TitlePart;
        }
    }

    public class AutoRoutePartType : AutoRegisteringObjectGraphType<AutoroutePart>
    {
        public AutoRoutePartType()
        {
            Name = "autoroutepart";

            Interface<ContentPartInterface>();

            IsTypeOf = value => value is AutoroutePart;
        }
    }

    public class BagPartType : ObjectGraphType<BagPart>
    {
        public BagPartType()
        {
            Name = "bagpart";

            Field<ListGraphType<ContentItemType>>("contentitems", resolve: 
                context => context.Source.ContentItems
            );

            Interface<ContentPartInterface>();

            IsTypeOf = value => value is BagPart;
        }
    }
}
