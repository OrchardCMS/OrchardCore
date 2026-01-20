using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.Drivers;

public sealed class ContentVersionedEventDisplayDriver : ContentEventDisplayDriver<ContentVersionedEvent, ContentVersionedEventViewModel>
{
    public ContentVersionedEventDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        : base(contentDefinitionManager)
    {
    }
}
