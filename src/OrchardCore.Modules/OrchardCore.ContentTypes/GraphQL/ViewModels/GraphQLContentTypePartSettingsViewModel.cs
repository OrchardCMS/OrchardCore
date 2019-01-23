using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Settings;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes.GraphQL.ViewModels
{
    public class GraphQLContentTypePartSettingsViewModel
    {
        public GraphQLContentOptions Options { get; set; }
        public GraphQLContentTypePartSettings Settings { get; set; }
        public ContentTypePartDefinition Definition { get; set; }
    }
}
