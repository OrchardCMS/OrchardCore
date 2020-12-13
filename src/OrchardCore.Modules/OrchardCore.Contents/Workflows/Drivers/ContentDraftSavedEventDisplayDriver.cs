using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentDraftSavedEventDisplayDriver : ContentEventDisplayDriverDriver<ContentDraftSavedEvent, ContentDraftSavedEventViewModel>
    {
        public ContentDraftSavedEventDisplayDriver(IContentDefinitionManager contentDefinitionManager) : base(contentDefinitionManager)
        {
        }
    }
}
