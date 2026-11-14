using System.Text.RegularExpressions;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentManagement.Display.ContentDisplay;

/// <summary>
/// Extensions describing how a part is rendered in the editor of a content item.
/// </summary>
public static partial class ContentTypePartDefinitionEditorExtensions
{
    /// <summary>
    /// Gets the name of the card holding the editor of a part in the editor of a content item, which is the title
    /// of the card: the display name of the part. A part without a display name of its own is named after its
    /// technical name, e.g. "HtmlBody", whose words are separated, e.g. "Html Body".
    /// </summary>
    /// <param name="typePartDefinition">The definition of the part in its content type.</param>
    /// <returns>The name of the card of the part.</returns>
    public static string GetEditorCardName(this ContentTypePartDefinition typePartDefinition)
    {
        ArgumentNullException.ThrowIfNull(typePartDefinition);

        var displayName = typePartDefinition.DisplayName();

        if (string.IsNullOrEmpty(typePartDefinition.GetSettings<ContentTypePartSettings>().DisplayName) &&
            string.IsNullOrEmpty(typePartDefinition.PartDefinition.GetSettings<ContentPartSettings>().DisplayName) &&
            typePartDefinition.PartDefinition.Name != typePartDefinition.ContentTypeDefinition.Name)
        {
            displayName = CamelCaseWordBoundaryRegex().Replace(displayName, " ");
        }

        return displayName;
    }

    [GeneratedRegex("(?<=[a-z])(?=[A-Z])")]
    private static partial Regex CamelCaseWordBoundaryRegex();
}
