using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Conventions;
using GraphQL.Conventions.Relay;
using GraphQL.Types;
using OrchardCore.Autoroute.Model;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.MetaData;
using OrchardCore.Flows.Models;
using OrchardCore.Title.Model;

namespace OrchardCore.RestApis.Types
{
    public class ContentItemType : AutoRegisteringObjectGraphType<ContentItem>
    {
        public ContentItemType(
            IEnumerable<ContentPart> contentParts,
            IContentDefinitionManager contentDefinitionManager)
        {
            Name = "contentitem";

            //Field("id", h => h.ContentItemId).Description("The id of the content item.");

            //Field<ListGraphType>(
            //    "metadata",
            //    resolve: context => 
            //    );


            Field<ListGraphType<ContentPartInterface>>(
          "parts",
          resolve: context =>
          {

              var typeDefinition = contentDefinitionManager.GetTypeDefinition(context.Source.ContentType);

              var typeParts = new List<ContentElement>();

              foreach (var part in typeDefinition.Parts)
              {
                  var name = part.Name; // About
                        var partName = part.PartDefinition.Name; // BagPart

                        var contentPart = contentParts.FirstOrDefault(x => x.GetType().Name == partName);

                  if (contentPart != null)
                  {
                      typeParts.Add(context
                          .Source
                          .Get(
                              contentPart.GetType(),
                              name));
                  }
              }

              return typeParts;
          });



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
        }
    }

    public class AutoRoutePartType : AutoRegisteringObjectGraphType<AutoroutePart>
    {
        public AutoRoutePartType()
        {
            Name = "autoroutepart";

            Interface<ContentPartInterface>();
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
        }
    }
}
