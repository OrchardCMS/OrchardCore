using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace OrchardCore.ContentManagement.Metadata.Models;

public class ContentTypeDefinition : ContentDefinition
{
    public ContentTypeDefinition(string name, string displayName, IEnumerable<ContentTypePartDefinition> parts, JsonObject settings)
    {
        Name = name;
        DisplayName = displayName;
        Parts = parts.ToList();
        Settings = settings.Clone();

        foreach (var part in Parts)
        {
            part.ContentTypeDefinition = this;
        }
    }

    public ContentTypeDefinition(string name, string displayName)
    {
        Name = name;
        DisplayName = displayName;
        Parts = [];
        Settings = [];
    }

    [Required, StringLength(1024)]
    public string DisplayName { get; private set; }

    public IEnumerable<ContentTypePartDefinition> Parts { get; private set; }

    /// <summary>
    /// Returns the <see cref="DisplayName"/> value of the type if defined,
    /// or the <see cref="ContentDefinition.Name"/> otherwise.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return string.IsNullOrEmpty(DisplayName)
            ? Name
            : DisplayName;
    }
}
