using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Conventions;
using GraphQL.Conventions.Relay;
using GraphQL.Types;
using OrchardCore.Autoroute.Model;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.MetaData;
using OrchardCore.Title.Model;

namespace OrchardCore.RestApis.Types
{
    //[ImplementViewer(OperationType.Query)]
    //public class Query
    //{
    //    [Description("Retrieve book/author by its globally unique ID.")]
    //    public Task<ContentItemNode> ContentItem(Id id) =>
    //        context.Get<INode>(id);
    //}

    //[Name("contentitem")]
    //public class ContentItemNode : INode
    //{
    //    private readonly ContentItem _dto;

    //    public ContentItemNode(ContentItem dto)
    //    {
    //        _dto = dto;
            
    //    }

    //    public Id Id => _dto.ContentItemId;

    //    public List<IContentPartNode> Parts { get; set; }
    //}

    //public interface IContentPartNode : INode
    //{
    //}

    //public class TitlePartNode : IContentPartNode
    //{
    //    public Id Id => Id.IdentifierForType<TitlePartNode>();


    //}


    public class ContentItemType : AutoRegisteringObjectGraphType<ContentItem>
    {
        public ContentItemType(
            IEnumerable<ContentPart> contentParts,
            IContentDefinitionManager contentDefinitionManager)
        {
            Name = "contentitem";

            //Field("id", h => h.ContentItemId).Description("The id of the content item.");

            Field<ListGraphType<ContentPartInterface>>(
                "parts",
                resolve: context =>
                {
                    var typeDefinition = contentDefinitionManager.GetTypeDefinition(context.Source.ContentType);

                    var typeParts = new List<ContentElement>();

                    foreach (var part in typeDefinition.Parts)
                    {
                        var foundPart = contentParts.FirstOrDefault(cp => cp.GetType().Name == part.Name);
                        
                        if (foundPart != null)
                        {
                            typeParts.Add(context.Source.Get(foundPart.GetType(), part.PartDefinition.Name));
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
}
