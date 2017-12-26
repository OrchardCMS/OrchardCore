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
        public string ApplicationId { get; set; }
        public string DisplayName { get; set; }
        public bool IsChecked { get; set; }
    }
}