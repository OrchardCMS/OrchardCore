using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentCreatedEventDisplay : ContentEventDisplayDriver<ContentCreatedEvent, ContentCreatedEventViewModel>
    {
        public ContentCreatedEventDisplay(IContentDefinitionManager contentDefinitionManager) : base(contentDefinitionManager)
        {
        }
    }
}
