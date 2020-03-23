using System.Collections.Generic;

namespace OrchardCore.Queries
{
    public interface IQueryResults
    {
        public IEnumerable<object> Items { get; set; }
    }
}
