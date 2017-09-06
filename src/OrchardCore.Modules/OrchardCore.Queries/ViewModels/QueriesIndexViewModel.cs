using System.Collections.Generic;

namespace OrchardCore.Queries.ViewModels
{
    public class QueriesIndexViewModel
    {
        public IList<QueryEntry> Queries { get; set; }
        public QueryIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public IEnumerable<string> QuerySourceNames { get; set; }
    }

    public class QueryEntry
    {
        public Query Query { get; set; }
        public bool IsChecked { get; set; }
        public dynamic Shape { get; set; }
    }

    public class QueryIndexOptions
    {
        public string Search { get; set; }
    }
}
