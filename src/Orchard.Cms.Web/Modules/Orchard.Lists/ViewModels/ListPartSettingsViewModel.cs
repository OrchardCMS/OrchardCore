using System.Collections.Specialized;
using Orchard.Lists.Models;

namespace Orchard.Lists.ViewModels
{
    public class ListPartSettingsViewModel
    {
        public ListPartSettings ListPartSettings { get; set; }
        public NameValueCollection ContentTypes { get; set; }
        public string ContainedContentType { get; set; }
    }
}
