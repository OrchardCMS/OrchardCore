using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Search.Lucene.ViewModels
{
    public class QueryIndexViewModel
    {
        public string Query { get; set; }
        public string IndexName { get; set; }

        [BindNever]
        public TimeSpan Duration { get; set; }

        [BindNever]
        public IEnumerable<Document> Documents { get; set; } = Enumerable.Empty<Document>();
    }
}
