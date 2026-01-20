using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Search.ViewModels
{
    public class SearchIndexViewModel : ShapeViewModel
    {
        public SearchIndexViewModel()
            : base("Search__List")
        {
        }

        public SearchIndexViewModel(string shapeType)
            : base(shapeType)
        {
        }

        public string Terms { get; set; }

        public string Index { get; set; }

        [BindNever]
        public string PageTitle { get; set; }

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
