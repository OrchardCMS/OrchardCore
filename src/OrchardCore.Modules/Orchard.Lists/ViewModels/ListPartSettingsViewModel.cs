using System.Collections.Specialized;
using Orchard.Lists.Models;

namespace Orchard.Lists.ViewModels
{
    public class ListPartSettingsViewModel
    {
        public ListPartSettings ListPartSettings { get; set; }
        public NameValueCollection ContentTypes { get; set; }
        public string[] ContainedContentTypes { get; set; }
        public int PageSize { get; set; }
    }
}
