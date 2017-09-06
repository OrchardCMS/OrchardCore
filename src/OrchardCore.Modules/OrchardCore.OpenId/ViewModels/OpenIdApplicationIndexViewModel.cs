using System.Collections.Generic;
using OrchardCore.OpenId.Models;

namespace OrchardCore.OpenId.ViewModels
{
    public class OpenIdApplicationsIndexViewModel
    {
        public IList<OpenIdApplicationEntry> Applications { get; set; }
        public dynamic Pager { get; set; }
    }

    public class OpenIdApplicationEntry
    {
        public OpenIdApplication Application { get; set; }
        public bool IsChecked { get; set; }
    }
}