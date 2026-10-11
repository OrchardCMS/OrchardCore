namespace OrchardCore.DataSources;

/// <summary>
/// Gives access to the registered data sources.
/// </summary>
public interface IDataSourceManager
{
    /// <summary>
    /// Lists the registered data sources, ordered by display name.
    /// </summary>
    /// <returns>The data sources.</returns>
    IReadOnlyList<IDataSource> GetDataSources();

    /// <summary>
    /// Finds a data source by its technical name, ignoring case.
    /// </summary>
    /// <param name="name">The technical name of the data source.</param>
    /// <returns>The data source, or <see langword="null"/> when none is registered with that name.</returns>
    IDataSource GetDataSource(string name);
}
