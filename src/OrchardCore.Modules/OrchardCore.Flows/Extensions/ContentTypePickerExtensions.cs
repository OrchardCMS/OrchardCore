using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.Flows.Extensions;

public static class ContentTypePickerExtensions
{
    private const string DefaultCategory = "Common";

    /// <summary>
    /// Gets the category for the content type, defaulting to "Common" if not set.
    /// </summary>
    public static string GetCategory(this ContentTypeDefinition contentTypeDefinition)
    {
        var category = contentTypeDefinition.GetSettings<ContentTypeSettings>()?.Category;
        return !string.IsNullOrEmpty(category) ? category : DefaultCategory;
    }

    /// <summary>
    /// Gets the preview image path for the content type.
    /// Falls back to a convention-based path: ~/{themeId}/blocks/{contentTypeName}.png
    /// </summary>
    public static string GetPreviewImage(this ContentTypeDefinition contentTypeDefinition, string themeId)
    {
        var imagePath = contentTypeDefinition.GetSettings<ContentTypeSettings>()?.PreviewImagePath;

        if (!string.IsNullOrEmpty(imagePath))
        {
            return imagePath;
        }

        // Convention-based fallback path
        return $"~/{themeId}/blocks/{contentTypeDefinition.Name}.png";
    }
}
