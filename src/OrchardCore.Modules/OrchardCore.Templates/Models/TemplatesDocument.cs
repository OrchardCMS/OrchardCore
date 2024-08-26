using OrchardCore.Data.Documents;

namespace OrchardCore.Templates.Models;

public class TemplatesDocument : Document
{
    private readonly Dictionary<string, Template> _templates = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, Template> Templates
    {
        get => _templates;
        
        set
        {
            _templates.Clear();

            if (value != null)
            {
                foreach (var (key, template) in value)
                {
                    _templates[key] = template;
                }
            }
        }
    }
}

public class Template
{
    public string Content { get; set; }
    public string Description { get; set; }
}
