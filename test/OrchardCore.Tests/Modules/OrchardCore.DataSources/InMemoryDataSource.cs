using System.Runtime.CompilerServices;
using OrchardCore.DataSources;

namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

/// <summary>
/// A data source over rows held in memory, for tests.
/// </summary>
internal sealed class InMemoryDataSource : IDataSource
{
    private readonly Dictionary<string, (DataField[] Fields, List<object[]> Rows)> _dataSets = new(StringComparer.Ordinal);

    public InMemoryDataSource(string name = "Memory", string displayName = null)
    {
        Name = name;
        DisplayName = new LocalizedString(name, displayName ?? name);
    }

    public string Name { get; }

    public LocalizedString DisplayName { get; }

    public LocalizedString Description => new(Name, string.Empty);

    public List<DataSourceQuery> Queries { get; } = [];

    public Func<DataSourceContext, bool> CanRead { get; set; } = _ => true;

    public InMemoryDataSource Add(string dataSet, DataField[] fields, params object[][] rows)
    {
        _dataSets[dataSet] = (fields, rows.ToList());

        return this;
    }

    public Task<IReadOnlyList<DataSetDescriptor>> GetDataSetsAsync(DataSourceContext context, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DataSetDescriptor> result = CanRead(context)
            ? _dataSets.Keys.Select(name => new DataSetDescriptor(name, name)).ToArray()
            : [];

        return Task.FromResult(result);
    }

    public Task<DataSetSchema> GetSchemaAsync(string dataSet, DataSourceContext context, CancellationToken cancellationToken = default)
    {
        if (!CanRead(context) || !_dataSets.TryGetValue(dataSet, out var data))
        {
            return Task.FromResult<DataSetSchema>(null);
        }

        return Task.FromResult(new DataSetSchema
        {
            DataSet = new DataSetDescriptor(dataSet, dataSet),
            Fields = data.Fields.ToList(),
        });
    }

    public async IAsyncEnumerable<DataBatch> ReadAsync(DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Queries.Add(query);

        if (!CanRead(query.Context) || !_dataSets.TryGetValue(query.DataSet, out var data))
        {
            yield break;
        }

        var rows = query.MaxRows > 0 ? data.Rows.Take(query.MaxRows) : data.Rows;

        foreach (var chunk in rows.Chunk(Math.Max(1, query.BatchSize)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Yield();

            yield return new DataBatch(data.Fields, chunk);
        }
    }
}
