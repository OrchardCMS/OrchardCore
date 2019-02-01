using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentUnpublishedEventDisplay : ContentEventDisplayDriver<ContentUnpublishedEvent, ContentUnpublishedEventViewModel>
    {
        public ContentUnpublishedEventDisplay(IContentDefinitionManager contentDefinitionManager) : base(contentDefinitionManager)
        {
        }
    }
}
