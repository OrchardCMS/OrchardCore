using System.Runtime.CompilerServices;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Operations;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Reads the rows of a record input as one sequence, laid out as the fields of its first batch: the rows of a batch
/// with other fields, such as from another connection, are mapped to them by name.
/// </summary>
public sealed class DataPipelineRowReader : IAsyncDisposable
{
    private readonly IAsyncEnumerator<DataBatch> _batches;
    private readonly CancellationToken _cancellationToken;
    private DataBatch _first;
    private bool _started;

    private DataPipelineRowReader(IReadOnlyList<DataField> fields, DataBatch first, IAsyncEnumerator<DataBatch> batches, CancellationToken cancellationToken)
    {
        Fields = fields;
        _first = first;
        _batches = batches;
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets the fields of the rows: those of the first batch, or the described fields of an input without rows.
    /// </summary>
    public IReadOnlyList<DataField> Fields { get; }

    /// <summary>
    /// Starts reading an input, up to its first batch.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The reader.</returns>
    public static async Task<DataPipelineRowReader> OpenAsync(DataPipelineInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var batches = input.ReadBatchesAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
        var first = await batches.MoveNextAsync() ? batches.Current : null;

        return new DataPipelineRowReader(first?.Fields ?? input.Fields, first, batches, cancellationToken);
    }

    /// <summary>
    /// Creates the options of the operations of a step that may write rows to temporary files, such as a sort: the
    /// memory limit of <see cref="DataPipelineOptions.StepMemoryLimitMegabytes"/>, and a directory of the temporary
    /// directory of the run, deleted with it.
    /// </summary>
    /// <param name="context">The context of the step.</param>
    /// <returns>The options.</returns>
    public static DataSpillOptions CreateSpillOptions(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var megabytes = Math.Max(1, DataPipelineFormulas.GetOptions(context.Services).StepMemoryLimitMegabytes);
        var root = string.IsNullOrEmpty(context.Run.TemporaryDirectory) ? Path.GetTempPath() : context.Run.TemporaryDirectory;

        return new DataSpillOptions(Path.Combine(root, "spill", Guid.NewGuid().ToString("n")), megabytes * 1024L * 1024L);
    }

    /// <summary>
    /// Reads the rows. Call it once.
    /// </summary>
    /// <returns>The rows.</returns>
    public async IAsyncEnumerable<object[]> ReadRowsAsync()
    {
        if (_started)
        {
            throw new InvalidOperationException("The rows of an input can be read once.");
        }

        _started = true;

        if (_first is null)
        {
            yield break;
        }

        foreach (var row in _first.Rows)
        {
            yield return row;
        }

        _first = null;

        while (await _batches.MoveNextAsync())
        {
            _cancellationToken.ThrowIfCancellationRequested();

            var batch = _batches.Current;
            var map = Map(batch.Fields);

            foreach (var row in batch.Rows)
            {
                yield return map is null ? row : Project(row, map);
            }
        }
    }

    /// <summary>
    /// Writes rows to an output in batches.
    /// </summary>
    /// <param name="output">The output.</param>
    /// <param name="fields">The fields of the rows.</param>
    /// <param name="rows">The rows.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The number of rows written.</returns>
    public static async Task<long> WriteRowsAsync(DataPipelineOutput output, IReadOnlyList<DataField> fields, IAsyncEnumerable<object[]> rows, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(rows);

        var count = 0L;
        var batch = new List<object[]>(DataSourceQuery.DefaultBatchSize);

        await foreach (var row in rows.WithCancellation(cancellationToken))
        {
            batch.Add(row);
            count++;

            if (batch.Count == DataSourceQuery.DefaultBatchSize)
            {
                await output.WriteAsync(new DataBatch(fields, batch), cancellationToken);
                batch = new List<object[]>(DataSourceQuery.DefaultBatchSize);
            }
        }

        if (batch.Count > 0)
        {
            await output.WriteAsync(new DataBatch(fields, batch), cancellationToken);
        }

        return count;
    }

    public ValueTask DisposeAsync() => _batches.DisposeAsync();

    private int[] Map(IReadOnlyList<DataField> fields)
    {
        if (ReferenceEquals(fields, Fields) ||
            (fields.Count == Fields.Count && fields.Select(field => field.Name).SequenceEqual(Fields.Select(field => field.Name), StringComparer.Ordinal)))
        {
            return null;
        }

        return Fields.Select(field => fields.IndexOf(field.Name)).ToArray();
    }

    private static object[] Project(object[] row, int[] map)
    {
        var projected = new object[map.Length];

        for (var index = 0; index < map.Length; index++)
        {
            var source = map[index];
            projected[index] = source >= 0 && source < row.Length ? row[source] : null;
        }

        return projected;
    }
}
