namespace OrchardCore.DataSources.Operations;

/// <summary>
/// Holds rows to read them later, any number of times, in the order they were added. The rows stay in memory until
/// they exceed <see cref="DataSpillOptions.MemoryLimit"/>; then they all go to a temporary file, deleted when the
/// buffer is disposed. Add every row before reading them.
/// </summary>
public sealed class DataRowBuffer : IDisposable
{
    private readonly DataSpillOptions _options;
    private List<object[]> _rows = [];
    private long _size;
    private DataRowFile _file;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataRowBuffer"/> class.
    /// </summary>
    /// <param name="options">The memory limit and the directory of the temporary file.</param>
    public DataRowBuffer(DataSpillOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <summary>
    /// Gets the number of rows added.
    /// </summary>
    public long Count { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the rows are in a temporary file rather than in memory.
    /// </summary>
    public bool IsSpilled => _file is not null;

    /// <summary>
    /// Gets the rows when they are in memory, or <see langword="null"/> when they are in a temporary file.
    /// </summary>
    internal IReadOnlyList<object[]> InMemoryRows => _file is null ? _rows : null;

    /// <summary>
    /// Adds a row.
    /// </summary>
    /// <param name="row">The row.</param>
    public void Add(object[] row)
    {
        Count++;

        if (_file is not null)
        {
            _file.Write(row);

            return;
        }

        _rows.Add(row);
        _size += DataRowSize.Estimate(row);

        if (_size > _options.MemoryLimit)
        {
            _file = DataRowFile.Create(_options.TemporaryDirectory);

            foreach (var buffered in _rows)
            {
                _file.Write(buffered);
            }

            _rows = null;
            _size = 0;
        }
    }

    /// <summary>
    /// Reads the rows in the order they were added.
    /// </summary>
    /// <returns>The rows.</returns>
    public IEnumerable<object[]> Read()
    {
        return _file is null ? _rows : _file.Read();
    }

    /// <summary>
    /// Deletes the temporary file, if any.
    /// </summary>
    public void Dispose()
    {
        _file?.Dispose();
        _file = null;
        _rows = null;
    }
}
