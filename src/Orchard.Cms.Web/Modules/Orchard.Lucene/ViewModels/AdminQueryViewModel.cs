using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Orchard.Lucene.ViewModels
{
    public class AdminQueryViewModel
    {
        public string Query { get; set; }
        public string IndexName { get; set; }
        public string[] FieldNames { get; set; }

        [BindNever]
        public IEnumerable<Document> Documents { get; set; } = Enumerable.Empty<Document>();
    }
}
