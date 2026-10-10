using Microsoft.Extensions.Localization;

namespace OrchardCore.DataSources;

/// <summary>
/// A connector that tabular data is read from. A data source exposes one or more data sets (for example the content
/// types of the tenant, its saved queries, or its users), describes the typed fields of each one, and streams their
/// rows in batches. Register an implementation as a scoped service with
/// <c>services.AddDataSource&lt;TDataSource&gt;()</c> to make its data sets available to every consumer.
/// </summary>
public interface IDataSource
{
    /// <summary>
    /// Gets the stable technical name of the data source. Saved definitions store this name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the label shown to users.
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// Gets the description shown to users.
    /// </summary>
    LocalizedString Description { get; }

    /// <summary>
    /// Lists the data sets the principal in <paramref name="context"/> is allowed to read.
    /// </summary>
    /// <param name="context">The caller context.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The accessible data sets.</returns>
    Task<IReadOnlyList<DataSetDescriptor>> GetDataSetsAsync(DataSourceContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Describes the fields of a data set.
    /// </summary>
    /// <param name="dataSet">The technical name of the data set.</param>
    /// <param name="context">The caller context.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// The schema, or <see langword="null"/> when the data set does not exist or the principal in
    /// <paramref name="context"/> is not allowed to read it. Consumers read a data set only after this method returns
    /// its schema, so it is the security boundary of a data source.
    /// </returns>
    Task<DataSetSchema> GetSchemaAsync(string dataSet, DataSourceContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the rows of a data set, one batch at a time, so any number of rows can be read without holding them all
    /// in memory. A data source checks again that the principal of <see cref="DataSourceQuery.Context"/> may read the
    /// data set, and yields nothing when it may not.
    /// </summary>
    /// <param name="query">The data set, fields, optional conditions, row limit and batch size to read.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The batches of rows read.</returns>
    IAsyncEnumerable<DataBatch> ReadAsync(DataSourceQuery query, CancellationToken cancellationToken = default);
}
