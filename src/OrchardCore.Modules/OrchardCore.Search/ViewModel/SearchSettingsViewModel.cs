using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.ViewModels
{
    public class SearchSettingsViewModel
    {
        public string SearchProviderAreaName { get; set; }

        [BindNever]
        public IEnumerable<SearchProvider> SearchProviders { get; set; } = Enumerable.Empty<SearchProvider>();
    }
}
