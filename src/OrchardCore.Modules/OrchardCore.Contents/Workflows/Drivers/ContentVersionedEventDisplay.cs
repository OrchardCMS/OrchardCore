using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentVersionedEventDisplay : ContentEventDisplayDriver<ContentVersionedEvent, ContentVersionedEventViewModel>
    {
        public ContentVersionedEventDisplay(IContentDefinitionManager contentDefinitionManager) : base(contentDefinitionManager)
        {

        }
    }
}
