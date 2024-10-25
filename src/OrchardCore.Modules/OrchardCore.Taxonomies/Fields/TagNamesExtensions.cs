using System.Text.Json.Nodes;

namespace OrchardCore.Taxonomies.Fields;

public static class TagNamesExtensions
{
    /// <summary>
    /// Tag names are a less well known property of a taxonomy field
    /// managed by the tags editor and tags display mode. 
    /// </summary>
    public static string[] GetTagNames(this TaxonomyField taxonomyField)
    {
        var tagNames = (JsonArray)taxonomyField.Content["TagNames"];

        return tagNames != null ? tagNames.ToObject<string[]>() : [];
    }

    /// <summary>
    /// Tags names are a less well known property of a taxonomy field
    /// managed by the tags editor and tags display mode.
    /// They are updated only when saving the content item.
    /// </summary>
    public static void SetTagNames(this TaxonomyField taxonomyField, string[] tagNames)
    {
        taxonomyField.Content["TagNames"] = JArray.FromObject(tagNames);
    }
}
