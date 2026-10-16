using OrchardCore.ContentManagement;
using OrchardCore.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Variables;

/// <summary>
/// A content item. Workflow instances store it by id (see <c>ContentItemSerializer</c>) and load its latest
/// version when they resume.
/// </summary>
public sealed class ContentItemVariableType : IWorkflowVariableType
{
    public static string Name => "contentItem";

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ContentItemVariableType>("Content item");

    public static string Editor => "none";

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
