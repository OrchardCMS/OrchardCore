using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.DataSources;

/// <summary>
/// The default <see cref="IDataSourceManager"/>, built from the data sources of <see cref="DataSourceOptions"/>. It
/// lists them from their descriptors, and resolves a data source from the services of the scope only when it is used.
/// </summary>
public sealed class DataSourceManager : IDataSourceManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyList<DataSourceDescriptor> _dataSources;
    private readonly Dictionary<string, DataSourceDescriptor> _byName;

    public DataSourceManager(IOptions<DataSourceOptions> options, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(options);

        _serviceProvider = serviceProvider;
        _dataSources = options.Value.DataSources;
        _byName = new Dictionary<string, DataSourceDescriptor>(StringComparer.OrdinalIgnoreCase);

        foreach (var dataSource in _dataSources)
        {
            if (!_byName.TryAdd(dataSource.Name, dataSource))
            {
                throw new InvalidOperationException($"More than one data source is registered with the name '{dataSource.Name}'.");
            }
        }
    }

    public IReadOnlyList<DataSourceDescriptor> GetDataSources() => _dataSources;

    public DataSourceDescriptor GetDescriptor(string name)
    {
        return string.IsNullOrEmpty(name) ? null : _byName.GetValueOrDefault(name);
    }

    public IDataSource GetDataSource(string name)
    {
        return GetDescriptor(name) is { } descriptor
            ? (IDataSource)_serviceProvider.GetRequiredService(descriptor.Type)
            : null;
    }
}
