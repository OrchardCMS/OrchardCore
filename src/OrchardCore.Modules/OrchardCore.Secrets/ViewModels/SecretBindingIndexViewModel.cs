using System.Collections.Generic;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.ViewModels
{
    public class SecretBindingIndexViewModel
    {
        public IList<SecretBindingEntry> SecretBindings { get; set; }
        public Dictionary<string, dynamic> Thumbnails { get; set; }
        public dynamic Pager { get; set; }
    }

    public class SecretBindingEntry
    {
        public string Name { get; set; }
        public SecretBinding SecretBinding { get; set; }
        public bool IsChecked { get; set; }
    }
}
