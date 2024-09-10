using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace OrchardCore.Contents.Workflows.ViewModels
{

    public class ContentForEachTaskViewModel
    {
        public bool QueriesEnabled { get; set; }
        public bool UseQuery { get; set; }
        public string ContentType { get; set; }
        public string QuerySource { get; set; }
        public string Query { get; set; }
        public string Parameters { get; set; }
        public bool PublishedOnly { get; set; }
        public int PageSize { get; set; }

        [BindNever]
        public IList<SelectListItem> AvailableContentTypes { get; set; }

        [BindNever]
        public List<SelectListItem> QuerySources { get; set; }

        [BindNever]
        public Dictionary<string, List<SelectListItem>> QueriesBySource { get; set; }
    }

}
