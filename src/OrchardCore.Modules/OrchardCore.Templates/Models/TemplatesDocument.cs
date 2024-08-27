using System.Text.Json.Serialization;
using OrchardCore.Data.Documents;
using OrchardCore.Json.Serialization;

namespace OrchardCore.Templates.Models;

public class TemplatesDocument : Document
{
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<Template>))]
    public Dictionary<string, Template> Templates { get; init; }
}

public class Template
{
    public string Content { get; set; }
    public string Description { get; set; }
}
