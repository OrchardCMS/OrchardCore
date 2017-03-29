using System.Collections.Generic;
using Orchard.OpenId.Models;

namespace Orchard.OpenId.ViewModels
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