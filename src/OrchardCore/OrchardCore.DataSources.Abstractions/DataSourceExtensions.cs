namespace OrchardCore.DataSources;

/// <summary>
/// Provides helpers to read data sources.
/// </summary>
public static class DataSourceExtensions
{
    /// <summary>
    /// Reads the rows of a data set into memory, up to <paramref name="maxRows"/>.
    /// </summary>
    /// <param name="dataSource">The data source to read.</param>
    /// <param name="query">The query. Its <see cref="DataSourceQuery.MaxRows"/> is replaced.</param>
    /// <param name="maxRows">The most rows to keep.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The rows read, and whether the data set had more.</returns>
    public static async Task<DataRowSet> ReadRowsAsync(
        this IDataSource dataSource,
        DataSourceQuery query,
        int maxRows,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRows);

        // Ask one more row than kept, to tell whether the data set has more.
        query.MaxRows = maxRows == int.MaxValue ? maxRows : maxRows + 1;

        var result = new DataRowSet();

        await foreach (var batch in dataSource.ReadAsync(query, cancellationToken))
        {
            result.Fields = batch.Fields;

            foreach (var row in batch.Rows)
            {
                if (result.Rows.Count == maxRows)
                {
                    result.Truncated = true;

                    return result;
                }

                result.Rows.Add(row);
            }
        }

        return result;
    }
}
