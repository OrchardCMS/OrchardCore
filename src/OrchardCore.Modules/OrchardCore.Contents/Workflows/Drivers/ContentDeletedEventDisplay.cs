using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentDeletedEventDisplay : ContentEventDisplayDriver<ContentDeletedEvent, ContentDeletedEventViewModel>
    {
        public ContentDeletedEventDisplay(IContentDefinitionManager contentDefinitionManager) : base(contentDefinitionManager)
        {
        }
    }
}
