using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Search.Lucene.ViewModels
{
    public class AdminQueryViewModel
    {
        public string DecodedQuery { get; set; }
        public string IndexName { get; set; }
        public string Parameters { get; set; }

        [BindNever]
        public int Count { get; set; }

        [BindNever]
        public string[] Indices { get; set; }

        [BindNever]
        public TimeSpan Elapsed { get; set; } = TimeSpan.Zero;

        [BindNever]
        public IEnumerable<Document> Documents { get; set; } = Enumerable.Empty<Document>();
    }
}
