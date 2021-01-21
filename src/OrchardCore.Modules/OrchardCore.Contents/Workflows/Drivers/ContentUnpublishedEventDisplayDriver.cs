using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentUnpublishedEventDisplayDriver : ContentEventDisplayDriver<ContentUnpublishedEvent, ContentUnpublishedEventViewModel>
    {
        public ContentUnpublishedEventDisplayDriver(IContentDefinitionManager contentDefinitionManager) : base(contentDefinitionManager)
        {
        }
    }
}
