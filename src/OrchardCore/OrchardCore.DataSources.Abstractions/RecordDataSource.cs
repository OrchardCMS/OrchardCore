using System.Runtime.CompilerServices;

namespace OrchardCore.DataSources;

/// <summary>
/// The base of a data source whose data sets read .NET records, such as documents or the rows of an index. Derive from
/// it, implement <see cref="IDataSource"/> with the static <c>Name</c>, <c>DisplayName</c> and <c>Description</c> of
/// the data source, list its data sets, each a <see cref="RecordDataSet{TRecord}"/>, and register it with
/// <c>services.AddDataSource&lt;TDataSource&gt;()</c>. It lists, describes and reads only the data sets the user may
/// read.
/// </summary>
public abstract class RecordDataSource
{
    /// <summary>
    /// Gets the data sets of the source.
    /// </summary>
    protected abstract IEnumerable<IRecordDataSet> DataSets { get; }

    public async Task<IReadOnlyList<DataSetDescriptor>> GetDataSetsAsync(DataSourceContext context, CancellationToken cancellationToken = default)
    {
        var descriptors = new List<DataSetDescriptor>();

        foreach (var dataSet in DataSets)
        {
            if (await dataSet.CanReadAsync(context ?? new DataSourceContext(), cancellationToken))
            {
                descriptors.Add(dataSet.Descriptor);
            }
        }

        return descriptors;
    }

    public async Task<DataSetSchema> GetSchemaAsync(string dataSet, DataSourceContext context, CancellationToken cancellationToken = default)
    {
        var set = Find(dataSet);

        if (set is null || !await set.CanReadAsync(context ?? new DataSourceContext(), cancellationToken))
        {
            return null;
        }

        return new DataSetSchema
        {
            DataSet = set.Descriptor,
            Fields = [.. set.Fields],
        };
    }

    public async IAsyncEnumerable<DataBatch> ReadAsync(DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var set = Find(query.DataSet);

        if (set is null || !await set.CanReadAsync(query.Context ?? new DataSourceContext(), cancellationToken))
        {
            yield break;
        }

        await foreach (var batch in set.ReadAsync(query, cancellationToken))
        {
            yield return batch;
        }
    }

    private IRecordDataSet Find(string name)
    {
        return string.IsNullOrEmpty(name)
            ? null
            : DataSets.FirstOrDefault(dataSet => string.Equals(dataSet.Descriptor?.Name, name, StringComparison.Ordinal));
    }
}

/// <summary>
/// A data set of a <see cref="RecordDataSource"/>.
/// </summary>
public interface IRecordDataSet
{
    /// <summary>
    /// Gets the description of the data set.
    /// </summary>
    DataSetDescriptor Descriptor { get; }

    /// <summary>
    /// Gets the fields of the data set.
    /// </summary>
    IReadOnlyList<DataField> Fields { get; }

    /// <summary>
    /// Tells whether the user of a context may read the data set.
    /// </summary>
    /// <param name="context">The caller context.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns><see langword="true"/> when the user may read the data set.</returns>
    Task<bool> CanReadAsync(DataSourceContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the rows of the data set. The user was checked before.
    /// </summary>
    /// <param name="query">The fields, conditions, row limit and batch size to read.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The batches of rows.</returns>
    IAsyncEnumerable<DataBatch> ReadAsync(DataSourceQuery query, CancellationToken cancellationToken = default);
}

/// <summary>
/// A data set whose rows are .NET records: declare its fields with <see cref="AddField"/>, each reading a value of a
/// record, and load the records with <see cref="LoadAsync"/>. The values are converted to the type of their field:
/// an enumeration value becomes its name, and a date becomes a UTC date.
/// </summary>
/// <typeparam name="TRecord">The type of the records.</typeparam>
public abstract class RecordDataSet<TRecord> : IRecordDataSet
{
    private readonly List<DataField> _fields = [];
    private readonly List<Func<TRecord, object>> _readers = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordDataSet{TRecord}"/> class.
    /// </summary>
    /// <param name="descriptor">The description of the data set.</param>
    protected RecordDataSet(DataSetDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        Descriptor = descriptor;
    }

    public DataSetDescriptor Descriptor { get; }

    public IReadOnlyList<DataField> Fields => _fields;

    /// <summary>
    /// Tells whether the user of a context may read the data set. Everyone may by default.
    /// </summary>
    /// <param name="context">The caller context.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns><see langword="true"/> when the user may read the data set.</returns>
    public virtual Task<bool> CanReadAsync(DataSourceContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public async IAsyncEnumerable<DataBatch> ReadAsync(DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var selected = Enumerable.Range(0, _fields.Count)
            .Where(index => query.Fields is null || query.Fields.Count == 0 || query.Fields.Contains(_fields[index].Name))
            .ToArray();

        var fields = selected.Select(index => _fields[index]).ToArray();
        var batchSize = query.BatchSize > 0 ? query.BatchSize : DataSourceQuery.DefaultBatchSize;
        var remaining = query.MaxRows > 0 ? query.MaxRows : long.MaxValue;
        var rows = new List<object[]>(batchSize);
        var any = false;

        await foreach (var record in LoadAsync(query, cancellationToken).WithCancellation(cancellationToken))
        {
            if (remaining-- <= 0)
            {
                break;
            }

            var row = new object[selected.Length];

            for (var index = 0; index < selected.Length; index++)
            {
                row[index] = DataValues.Coerce(Normalize(_readers[selected[index]](record)), fields[index].Type);
            }

            rows.Add(row);

            if (rows.Count == batchSize)
            {
                any = true;

                yield return new DataBatch(fields, rows);

                rows = new List<object[]>(batchSize);
            }
        }

        // An empty data set still yields its fields.
        if (rows.Count > 0 || !any)
        {
            yield return new DataBatch(fields, rows);
        }
    }

    /// <summary>
    /// Declares a field of the data set.
    /// </summary>
    /// <param name="name">The stable technical name of the field.</param>
    /// <param name="displayName">The label shown to users.</param>
    /// <param name="type">The type of the field values.</param>
    /// <param name="read">Reads the value of the field from a record.</param>
    /// <param name="group">The optional group the field is listed under.</param>
    /// <returns>The field, to set its other properties, such as <see cref="DataField.IsKeyFilterable"/> when
    /// <see cref="LoadAsync"/> applies <see cref="DataConditions.Keys"/> on it.</returns>
    protected DataField AddField(string name, string displayName, DataFieldType type, Func<TRecord, object> read, string group = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(read);

        if (_fields.Any(field => string.Equals(field.Name, name, StringComparison.Ordinal)))
        {
            throw new ArgumentException($"The data set '{Descriptor.Name}' already has a field named '{name}'.", nameof(name));
        }

        var field = new DataField(name, displayName ?? name, type, group);

        _fields.Add(field);
        _readers.Add(read);

        return field;
    }

    /// <summary>
    /// Loads the records, in a stable order, a page at a time so that any number of records can be read. It may apply
    /// the conditions of the query it understands, such as with <see cref="DataConditions"/>; the consumer applies
    /// every condition again. Stop when the enumeration stops: the base class stops reading at
    /// <see cref="DataSourceQuery.MaxRows"/>.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The records.</returns>
    protected abstract IAsyncEnumerable<TRecord> LoadAsync(DataSourceQuery query, CancellationToken cancellationToken);

    private static object Normalize(object value)
    {
        return value switch
        {
            Enum enumeration => enumeration.ToString(),
            DateTimeOffset date => date.UtcDateTime,
            DateTime { Kind: DateTimeKind.Unspecified } date => DateTime.SpecifyKind(date, DateTimeKind.Utc),
            DateTime { Kind: DateTimeKind.Local } date => date.ToUniversalTime(),
            _ => value,
        };
    }
}
