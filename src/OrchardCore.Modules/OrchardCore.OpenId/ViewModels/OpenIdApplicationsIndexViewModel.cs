using System.Collections.Generic;

namespace OrchardCore.OpenId.ViewModels
{
    public class OpenIdApplicationsIndexViewModel
    {
        public IList<OpenIdApplicationEntry> Applications { get; } = new List<OpenIdApplicationEntry>();
        public dynamic Pager { get; set; }
    }

    public class OpenIdApplicationEntry
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public bool IsChecked { get; set; }
    }
}
