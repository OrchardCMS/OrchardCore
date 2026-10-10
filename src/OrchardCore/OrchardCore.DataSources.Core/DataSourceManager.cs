namespace OrchardCore.DataSources;

/// <summary>
/// The default <see cref="IDataSourceManager"/>, built from the registered <see cref="IDataSource"/> services.
/// </summary>
public sealed class DataSourceManager : IDataSourceManager
{
    private readonly IReadOnlyList<IDataSource> _dataSources;
    private readonly Dictionary<string, IDataSource> _byName;

    public DataSourceManager(IEnumerable<IDataSource> dataSources)
    {
        ArgumentNullException.ThrowIfNull(dataSources);

        _byName = new Dictionary<string, IDataSource>(StringComparer.OrdinalIgnoreCase);

        foreach (var dataSource in dataSources)
        {
            if (!_byName.TryAdd(dataSource.Name, dataSource))
            {
                throw new InvalidOperationException($"More than one data source is registered with the name '{dataSource.Name}'.");
            }
        }

        _dataSources = _byName.Values
            .OrderBy(dataSource => dataSource.DisplayName?.Value ?? dataSource.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public IReadOnlyList<IDataSource> GetDataSources() => _dataSources;

    public IDataSource GetDataSource(string name)
        => string.IsNullOrEmpty(name) ? null : _byName.GetValueOrDefault(name);
}
