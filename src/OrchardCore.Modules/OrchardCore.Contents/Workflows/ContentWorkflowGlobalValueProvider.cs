using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows;

/// <summary>
/// The <c>Content</c> Liquid value, which loads content items by handle.
/// </summary>
public sealed class ContentWorkflowGlobalValueProvider : IWorkflowGlobalValueProvider
{
    internal readonly IStringLocalizer S;

    public ContentWorkflowGlobalValueProvider(IStringLocalizer<ContentWorkflowGlobalValueProvider> localizer)
    {
        S = localizer;
    }

    public IEnumerable<WorkflowGlobalValue> GetGlobalValues()
        =>
        [
            WorkflowGlobalValue.Liquid("Content", "object", S["The published content items, by handle: Content[\"alias:about\"]."],
            [
                new ActivityProvidedValueMember { Name = "Latest", TypeName = "object", Description = S["The latest versions, by handle: Content.Latest[\"alias:about\"]."] },
                new ActivityProvidedValueMember { Name = "ContentItemId", TypeName = "object", Description = S["The content items, by id."] },
                new ActivityProvidedValueMember { Name = "ContentItemVersionId", TypeName = "object", Description = S["The content item versions, by version id."] },
            ]),
        ];
}
