using System.Collections.Generic;

namespace OrchardCore.Queries
{
    /// <summary>
    /// Contracts for query results.
    /// </summary>
    public interface IQueryResults
    {
        /// <summary>
        /// Gets or sets the query items.
        /// </summary>
        public IEnumerable<object> Items { get; set; }
    }
}
