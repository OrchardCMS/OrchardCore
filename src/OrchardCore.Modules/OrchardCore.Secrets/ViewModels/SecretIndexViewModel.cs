using System.Collections.Generic;

namespace OrchardCore.Secrets.ViewModels;

public class SecretIndexViewModel
{
    public List<SecretInfoEntry> Entries { get; set; }
    public Dictionary<string, dynamic> Thumbnails { get; set; }
    public Dictionary<string, dynamic> Summaries { get; set; }
    public ContentOptions Options { get; set; } = new ContentOptions();
    public dynamic Pager { get; set; }
}
