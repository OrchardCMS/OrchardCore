namespace OrchardCore.DataSources;

/// <summary>
/// Gives access to the registered data sources.
/// </summary>
public interface IDataSourceManager
{
    /// <summary>
    /// Lists the registered data sources, in the order they were registered, without creating them.
    /// </summary>
    /// <returns>The descriptors of the data sources.</returns>
    IReadOnlyList<DataSourceDescriptor> GetDataSources();

    /// <summary>
    /// Finds the descriptor of a data source by its technical name, ignoring case.
    /// </summary>
    /// <param name="name">The technical name of the data source.</param>
    /// <returns>The descriptor, or <see langword="null"/> when none is registered with that name.</returns>
    DataSourceDescriptor GetDescriptor(string name);

    /// <summary>
    /// Gets a data source by its technical name, ignoring case, resolved from the services of the current scope.
    /// </summary>
    /// <param name="name">The technical name of the data source.</param>
    /// <returns>The data source, or <see langword="null"/> when none is registered with that name.</returns>
    IDataSource GetDataSource(string name);
}
