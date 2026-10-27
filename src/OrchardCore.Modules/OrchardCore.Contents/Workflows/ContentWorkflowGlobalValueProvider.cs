using OrchardCore.Localization;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows;

/// <summary>
/// The <c>Content</c> Liquid value, which loads content items by handle.
/// </summary>
public sealed class ContentWorkflowGlobalValueProvider : IWorkflowGlobalValueProvider
{
    // The values don't depend on the request or the culture, so they're created once.
    private static readonly WorkflowGlobalValue[] s_globalValues =
    [
        WorkflowGlobalValue.Liquid("Content", "object", LocalizationSource.Create<ContentWorkflowGlobalValueProvider>("The published content items, by handle: Content[\"alias:about\"]."),
        [
            new ActivityProvidedValueMember { Name = "Latest", TypeName = "object", Description = LocalizationSource.Create<ContentWorkflowGlobalValueProvider>("The latest versions, by handle: Content.Latest[\"alias:about\"].") },
            new ActivityProvidedValueMember { Name = "ContentItemId", TypeName = "object", Description = LocalizationSource.Create<ContentWorkflowGlobalValueProvider>("The content items, by id.") },
            new ActivityProvidedValueMember { Name = "ContentItemVersionId", TypeName = "object", Description = LocalizationSource.Create<ContentWorkflowGlobalValueProvider>("The content item versions, by version id.") },
        ]),
    ];

    public IEnumerable<WorkflowGlobalValue> GetGlobalValues()
        => s_globalValues;
}
