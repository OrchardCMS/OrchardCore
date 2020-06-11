using System.Collections.Generic;

namespace OrchardCore.OpenId.ViewModels
{
    public class OpenIdScopeIndexViewModel
    {
        public IList<OpenIdScopeEntry> Scopes { get; } = new List<OpenIdScopeEntry>();
        public dynamic Pager { get; set; }
    }

    public class OpenIdScopeEntry
    {
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public bool IsChecked { get; set; }
        public string Name { get; set; }
    }
}
