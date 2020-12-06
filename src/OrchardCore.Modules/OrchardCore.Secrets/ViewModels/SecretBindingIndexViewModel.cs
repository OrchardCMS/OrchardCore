using System.Collections.Generic;

namespace OrchardCore.Secrets.ViewModels
{
    public class SecretBindingIndexViewModel
    {
        public List<SecretBindingEntry> SecretBindings { get; set; }
        public Dictionary<string, dynamic> Thumbnails { get; set; }
        public Dictionary<string, dynamic> Summaries { get; set; }
        public dynamic Pager { get; set; }
    }

    public class SecretBindingEntry
    {
        public string Name { get; set; }
        public SecretBinding SecretBinding { get; set; }
        public bool IsChecked { get; set; }
        public dynamic Summary { get; set; }
    }
}
