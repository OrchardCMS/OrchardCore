using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentPublishedEventDisplayDriver : ContentEventDisplayDriver<ContentPublishedEvent, ContentPublishedEventViewModel>
    {
        public ContentPublishedEventDisplayDriver(IContentDefinitionManager contentDefinitionManager) : base(contentDefinitionManager)
        {
        }
    }
}
