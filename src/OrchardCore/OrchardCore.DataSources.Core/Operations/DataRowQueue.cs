namespace OrchardCore.DataSources.Operations;

/// <summary>
/// A first-in, first-out queue of batches of rows kept in a temporary file, for a producer that must not wait for a
/// slower consumer. The file is deleted when the queue is empty or disposed. The queue isn't thread-safe.
/// </summary>
public sealed class DataRowQueue : IDisposable
{
    private const int BufferSize = 64 * 1024;

    private readonly string _directory;
    private string _path;
    private FileStream _writeStream;
    private BinaryWriter _writer;
    private FileStream _readStream;
    private BinaryReader _reader;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataRowQueue"/> class.
    /// </summary>
    /// <param name="temporaryDirectory">The directory of the temporary file, created when needed.</param>
    public DataRowQueue(string temporaryDirectory)
    {
        ArgumentException.ThrowIfNullOrEmpty(temporaryDirectory);

        _directory = temporaryDirectory;
    }

    /// <summary>
    /// Gets the number of batches in the queue.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Adds a batch of rows at the end of the queue.
    /// </summary>
    /// <param name="rows">The rows.</param>
    public void Enqueue(IReadOnlyList<object[]> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        if (_writer is null)
        {
            Directory.CreateDirectory(_directory);
            _path = Path.Combine(_directory, $"queue-{Guid.NewGuid():n}.tmp");
            _writeStream = new FileStream(_path, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, BufferSize, FileOptions.DeleteOnClose);
            _writer = new BinaryWriter(_writeStream, System.Text.Encoding.UTF8, leaveOpen: true);
        }

        _writer.Write(rows.Count);

        foreach (var row in rows)
        {
            DataRowSerializer.Write(_writer, row);
        }

        // Make the batch visible to the reading handle.
        _writer.Flush();
        Count++;
    }

    /// <summary>
    /// Removes the batch at the start of the queue.
    /// </summary>
    /// <param name="rows">The rows of the batch.</param>
    /// <returns><see langword="true"/> when the queue had a batch.</returns>
    public bool TryDequeue(out IReadOnlyList<object[]> rows)
    {
        if (Count == 0)
        {
            rows = null;

            return false;
        }

        if (_reader is null)
        {
            _readStream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, BufferSize);
            _reader = new BinaryReader(_readStream, System.Text.Encoding.UTF8, leaveOpen: true);
        }

        var count = _reader.ReadInt32();
        var list = new List<object[]>(count);

        for (var index = 0; index < count; index++)
        {
            list.Add(DataRowSerializer.Read(_reader));
        }

        rows = list;
        Count--;

        // An empty queue starts a new file, so the space of the rows read is freed.
        if (Count == 0)
        {
            Close();
        }

        return true;
    }

    /// <summary>
    /// Deletes the temporary file.
    /// </summary>
    public void Dispose()
    {
        Close();
        Count = 0;
    }

    private void Close()
    {
        _reader?.Dispose();
        _readStream?.Dispose();
        _writer?.Dispose();
        _writeStream?.Dispose();

        _reader = null;
        _readStream = null;
        _writer = null;
        _writeStream = null;
    }
}
