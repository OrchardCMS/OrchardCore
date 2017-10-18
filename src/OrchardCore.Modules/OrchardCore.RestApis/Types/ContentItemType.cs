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
            Name = "ContentPart";
        }
    }

    public class TitlePartType : AutoRegisteringObjectGraphType<TitlePart>
    {
        public TitlePartType()
        {
            Name = typeof(TitlePart).Name;

            Interface<ContentPartInterface>();

            IsTypeOf = value => value is TitlePart;
        }
    }

    public class AutoRoutePartType : AutoRegisteringObjectGraphType<AutoroutePart>
    {
        public AutoRoutePartType()
        {
            Name = typeof(AutoroutePart).Name;

            Interface<ContentPartInterface>();

            IsTypeOf = value => value is AutoroutePart;
        }
    }

    public class BagPartType : ObjectGraphType<BagPart>
    {
        public BagPartType()
        {
            Name = typeof(BagPart).Name;

            Field<ListGraphType<ContentItemType>>("ContentItems", resolve: 
                context => context.Source.ContentItems
            );

            Interface<ContentPartInterface>();

            IsTypeOf = value => value is BagPart;
        }
    }
}
