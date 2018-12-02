using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL
{
    public static class ContentTypePartDefinitionExtensions
    {
        internal static bool ShouldCollapseFieldsToParent(this ContentTypePartDefinition part) {
            return part.GetSettings<GraphQLContentTypePartSettings>().CollapseFieldsToParent ||
                    part.ContentTypeDefinition.Name == part.PartDefinition.Name;
        } 
    }
}
