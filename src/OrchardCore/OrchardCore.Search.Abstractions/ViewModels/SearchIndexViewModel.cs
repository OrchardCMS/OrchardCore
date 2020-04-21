using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.Search.Abstractions.ViewModels
{
    [BindProperties(SupportsGet = true)]
    public class SearchIndexViewModel
    {
        public string Terms { get; set; }

        [BindNever]
        public SearchFormViewModel SearchForm { get; set; }

        [BindNever]
        public SearchResultsViewModel SearchResults { get; set; }

        [BindNever]
        public dynamic Pager { get; set; }

        [BindNever]
        public IEnumerable<ContentItem> ContentItems { get; set; }
    }
}
