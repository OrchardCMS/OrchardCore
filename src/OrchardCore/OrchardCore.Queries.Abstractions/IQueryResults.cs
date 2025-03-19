namespace OrchardCore.Queries;

/// <summary>
/// Contracts for query results.
/// </summary>
public interface IQueryResults
{
    /// <summary>
    /// Gets or sets the query items.
    /// </summary>
    IEnumerable<object> Items { get; set; }
}
