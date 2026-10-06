using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Variables;

/// <summary>
/// A content item. Workflow instances store it by id (see <c>ContentItemSerializer</c>) and load its latest
/// version when they resume.
/// </summary>
public sealed class ContentItemVariableType : IWorkflowVariableType
{
    internal readonly IStringLocalizer S;

    public ContentItemVariableType(IStringLocalizer<ContentItemVariableType> localizer)
    {
        S = localizer;
    }

    public string Name => "contentItem";

    public LocalizedString DisplayName => S["Content item"];

    public string Editor => "none";

    public bool TryCoerce(object value, out object result)
    {
        result = value switch
        {
            ContentItem contentItem => contentItem,
            IContent content => content.ContentItem,
            _ => null,
        };

        return value is null || result is not null;
    }
}
