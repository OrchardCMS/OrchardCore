using OrchardCore.Data.Documents;
using OrchardCore.Modules;

namespace OrchardCore.Templates.Models;

public class TemplatesDocument : Document
{
    private readonly Dictionary<string, Template> _templates = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, Template> Templates
    {
        get => _templates;
        
        set => _templates.SetItems(value);
    }
}

public class Template
{
    public string Content { get; set; }
    public string Description { get; set; }
}
