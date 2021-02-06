using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Queries.ViewModels
{
    public class QueriesIndexViewModel
    {
        public IList<QueryEntry> Queries { get; set; }
        public ContentOptions Options { get; set; } = new ContentOptions();
        public dynamic Pager { get; set; }
        public IEnumerable<string> QuerySourceNames { get; set; }
    }

    public class QueryEntry
    {
        public Query Query { get; set; }
        public bool IsChecked { get; set; }
        public dynamic Shape { get; set; }
    }

    public class ContentOptions
    {
        public string Search { get; set; }
        public ContentsBulkAction BulkAction { get; set; }

        #region Lists to populate

        [BindNever]
        public List<SelectListItem> ContentsBulkAction { get; set; }

        #endregion Lists to populate
    }

    public enum ContentsBulkAction
    {
        None,
        Remove
    }
}
