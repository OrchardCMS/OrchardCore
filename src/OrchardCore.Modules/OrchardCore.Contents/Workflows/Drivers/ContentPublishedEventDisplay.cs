using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentPublishedEventDisplay : ContentEventDisplayDriver<ContentPublishedEvent, ContentPublishedEventViewModel>
    {
        public ContentPublishedEventDisplay(IContentDefinitionManager contentDefinitionManager) : base(contentDefinitionManager)
        {
        }
    }
}
