using System.Collections.Generic;

namespace OrchardCore.Queries
{
    public interface IQueryResults<T>
    {
        public IEnumerable<T> Items { get; set; }
    }
}
