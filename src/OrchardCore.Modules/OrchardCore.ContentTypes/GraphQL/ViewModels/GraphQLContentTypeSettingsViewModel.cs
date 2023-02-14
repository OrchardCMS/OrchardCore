using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Settings;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes.GraphQL.ViewModels
{
    public class GraphQLContentTypeSettingsViewModel
    {
        public GraphQLContentTypeSettings Settings { get; set; }

        [BindNever]
        public GraphQLContentOptions Options { get; set; }

        [BindNever]
        public ContentTypeDefinition Definition { get; set; }
    }
}
