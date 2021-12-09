using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Search.Abstractions.ViewModels
{
    public class SearchResultsViewModel : ShapeViewModel
    {
        public SearchResultsViewModel(string shapeType) : base(shapeType)
        {
        }

        [BindNever]
        public IEnumerable<ContentItem> ContentItems { get; set; }
    }
}
