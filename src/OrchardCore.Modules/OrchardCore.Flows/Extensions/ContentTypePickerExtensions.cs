using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.Flows.Extensions;

public static class ContentTypePickerExtensions
{
    /// <summary>
    /// Gets the category for the content type, or null if not set.
    /// </summary>
    public static string GetCategory(this ContentTypeDefinition contentTypeDefinition)
    {
        var category = contentTypeDefinition.GetSettings<ContentTypeSettings>()?.Category;
        return !string.IsNullOrEmpty(category) ? category : null;
    }

    /// <summary>
    /// Gets the thumbnail path for the content type, or null if not configured.
    /// </summary>
    public static string GetThumbnail(this ContentTypeDefinition contentTypeDefinition)
    {
        return contentTypeDefinition.GetSettings<ContentTypeSettings>()?.ThumbnailPath;
    }
}
