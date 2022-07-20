using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Lucene.Services
{
    internal class LuceneSearchProvider : ISearchProvider
    {
        public string Name { get; } = "Lucene";
    }
}
