using System.Text.Json.Serialization;
using OrchardCore.Data.Documents;
using OrchardCore.Json.Serialization;

namespace OrchardCore.Shortcodes.Models;

public class ShortcodeTemplatesDocument : Document
{
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<ShortcodeTemplate>))]
    public Dictionary<string, ShortcodeTemplate> ShortcodeTemplates { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public class ShortcodeTemplate
{
    public string Content { get; set; }
    public string Hint { get; set; }
    public string Usage { get; set; }
    public string DefaultValue { get; set; }
    public string[] Categories { get; set; } = [];
}
