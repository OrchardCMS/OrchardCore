using System.Text.Json.Nodes;

namespace OrchardCore.ContentManagement.Metadata.Models;

public static class ContentPartFieldDefinitionExtensions
{
    /// <summary>
    /// Returns the value of the defined content field from the <paramref name="contentItem"/>.
    /// </summary>
    public static TField GetContentField<TField>(
        this ContentPartFieldDefinition fieldDefinition,
        ContentItem contentItem)
        where TField : ContentField
    {
        if (((JsonObject)contentItem.Content)[fieldDefinition.ContentTypePartDefinition.Name] is not JsonObject jPart ||
            jPart[fieldDefinition.Name] is not JsonObject jField)
        {
            return null;
        }

        return jField.ToObject<TField>();
    }

    /// <summary>
    /// Returns each field from <paramref name="fieldDefinitions"/> that exists in <paramref name="contentItem"/> in a
    /// tuple along with its <see cref="ContentPartFieldDefinition"/>.
    /// </summary>
    public static IEnumerable<(ContentPartFieldDefinition Definition, TField Field)> GetContentFields<TField>(
        this IEnumerable<ContentPartFieldDefinition> fieldDefinitions,
        ContentItem contentItem)
        where TField : ContentField
    {
        foreach (var fieldDefinition in fieldDefinitions)
        {
            var field = fieldDefinition.GetContentField<TField>(contentItem);
            if (field is not null)
            {
                yield return (fieldDefinition, field);
            }
        }
    }
}
