namespace OrchardCore.DataSources;

/// <summary>
/// Holds the registered data sources. <c>services.AddDataSource&lt;TDataSource&gt;()</c> adds them.
/// </summary>
public sealed class DataSourceOptions
{
    private readonly List<DataSourceDescriptor> _dataSources = [];

    /// <summary>
    /// Gets the registered data sources, in the order they were registered.
    /// </summary>
    public IReadOnlyList<DataSourceDescriptor> DataSources => _dataSources;

    /// <summary>
    /// Registers a data source, unless its class is already registered.
    /// </summary>
    /// <param name="descriptor">The data source.</param>
    /// <returns>The options.</returns>
    public DataSourceOptions Add(DataSourceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (!_dataSources.Any(dataSource => dataSource.Type == descriptor.Type))
        {
            _dataSources.Add(descriptor);
        }

        return this;
    }
}
