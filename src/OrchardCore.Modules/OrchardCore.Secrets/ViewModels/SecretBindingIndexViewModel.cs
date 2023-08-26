using System.Collections.Generic;

namespace OrchardCore.Secrets.ViewModels;

public class SecretBindingIndexViewModel
{
    public List<SecretBindingEntry> SecretBindings { get; set; }
    public Dictionary<string, dynamic> Thumbnails { get; set; }
    public Dictionary<string, dynamic> Summaries { get; set; }
    public ContentOptions Options { get; set; } = new ContentOptions();
    public dynamic Pager { get; set; }
}
