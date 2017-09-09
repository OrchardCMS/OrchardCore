using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Title.Model;

namespace Orchard.RestApis.Types
{
    public class ContentItemType : ObjectGraphType<ContentItem>
    {
        public ContentItemType(
            IEnumerable<ContentPart> contentParts,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<ObjectGraphType> objectGraphTypes)
        {
            Name = "contentitem";

            Field("id", h => h.ContentItemId).Description("The id of the content item.");

            Field(h => h.Published);
            Field(h => h.Latest);

            Field(h => h.Number);
            Field(h => h.ContentType);
            Field(h => h.ContentItemVersionId);

            Field<ListGraphType>(
                "parts",
                resolve: context =>
                {
                    var typeDefinition = contentDefinitionManager.GetTypeDefinition(context.Source.ContentType);

                    var typeParts = new UnionGraphType();

                    foreach (var part in typeDefinition.Parts)
                    {
                        var foundPart = contentParts.FirstOrDefault(cp => cp.GetType().Name == part.Name);

                        if (foundPart != null)
                        {
                            var ogt = objectGraphTypes.FirstOrDefault(x => x.Name == part.Name);

                            if (ogt != null)
                            {
                                typeParts.AddPossibleType(ogt);
                            }
                        }
                    }

                    return typeParts;
                }
                );
        }
    }

    public class TitlePartType : ObjectGraphType<TitlePart>
    {
        public TitlePartType()
        {
            Name = "TitlePart";

            Field(h => h.Title);
        }
    }
}
