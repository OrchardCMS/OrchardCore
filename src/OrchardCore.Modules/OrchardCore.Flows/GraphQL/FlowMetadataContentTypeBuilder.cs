using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.GraphQL
{
    public class FlowMetadataContentTypeBuilder : IContentTypeBuilder
    {
        public void Build(FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType)
        {
            var settings = contentTypeDefinition.GetSettings<ContentTypeSettings>();

            if (settings.Stereotype != "Widget") return;

            contentItemType.Field<FlowMetadataQueryObjectType>(
                "metadata",
                resolve: context => context.Source.As<FlowMetadata>()
            );
        }
    }
}
