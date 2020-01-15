using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.Search.Abstractions.ViewModels
{
    [BindProperties(SupportsGet = true)]
    public class SearchIndexViewModel
    {
        [FromQuery(Name = "Terms")]
        public string Terms { get; set; }

        [FromForm(Name = "IndexName")]
        public string IndexName { get; set; } = "Search";

        [BindNever]
        public dynamic Pager { get; set; }

        [BindNever]
        public IEnumerable<ContentItem> ContentItems { get; set; }
    }
}
