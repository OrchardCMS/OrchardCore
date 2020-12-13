using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentUnpublishedEventDisplayDriver : ContentEventDisplayDriverDriver<ContentUnpublishedEvent, ContentUnpublishedEventViewModel>
    {
        public ContentUnpublishedEventDisplayDriver(IContentDefinitionManager contentDefinitionManager) : base(contentDefinitionManager)
        {
        }
    }
}
