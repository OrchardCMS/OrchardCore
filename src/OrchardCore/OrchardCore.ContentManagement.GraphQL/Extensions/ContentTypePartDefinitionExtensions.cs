using OrchardCore.ContentManagement.GraphQL.Queries.Models;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL
{
    public static class ContentTypePartDefinitionExtensions
    {
        internal static bool ShouldCollapseFieldsToParent(this ContentTypePartDefinition part) {
            // When the part has the same name as the content type, it is the main part for
            // the content type's fields so we collapse them into the parent type.
            return part.GetSettings<GraphQLContentTypePartSettings>().CollapseFieldsToParent ||
                    part.ContentTypeDefinition.Name == part.PartDefinition.Name;
        } 
    }
}
