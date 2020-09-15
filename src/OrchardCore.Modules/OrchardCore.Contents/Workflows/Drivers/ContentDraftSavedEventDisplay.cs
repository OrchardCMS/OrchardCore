using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentDraftSavedEventDisplay : ContentEventDisplayDriver<ContentDraftSavedEvent, ContentDraftSavedEventViewModel>
    {
        public ContentDraftSavedEventDisplay(IContentDefinitionManager contentDefinitionManager) : base(contentDefinitionManager)
        {
        }
    }
}
