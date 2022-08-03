using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Search.ViewModels
{
    public class SearchSettingsViewModel
    {
        public string SearchProvider { get; set; }

        [BindNever]
        public IEnumerable<string> SearchProviders { get; set; } = new List<string>();
    }
}
