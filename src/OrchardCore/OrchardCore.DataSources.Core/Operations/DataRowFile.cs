namespace OrchardCore.DataSources.Operations;

/// <summary>
/// A temporary file of rows, written once and then read any number of times. The file is deleted when it is disposed,
/// or when the process ends.
/// </summary>
internal sealed class DataRowFile : IDisposable
{
    private const int BufferSize = 64 * 1024;

    private readonly FileStream _stream;
    private readonly BinaryWriter _writer;
    private bool _flushed;

    private DataRowFile(string path)
    {
        Path = path;

        // Readers open their own handles; sharing the deletion lets the file go away when this handle closes.
        _stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete, BufferSize, FileOptions.DeleteOnClose);
        _writer = new BinaryWriter(_stream, System.Text.Encoding.UTF8, leaveOpen: true);
    }

    public string Path { get; }

    public long Count { get; private set; }

    public static DataRowFile Create(string directory)
    {
        Directory.CreateDirectory(directory);

        return new DataRowFile(System.IO.Path.Combine(directory, $"rows-{Guid.NewGuid():n}.tmp"));
    }

    public void Write(object[] row)
    {
        _flushed = false;
        DataRowSerializer.Write(_writer, row);
        Count++;
    }

    /// <summary>
    /// Reads the rows written so far, in the order they were written.
    /// </summary>
    public IEnumerable<object[]> Read()
    {
        if (!_flushed)
        {
            _writer.Flush();
            _stream.Flush();
            _flushed = true;
        }

        var count = Count;

        return ReadRows(Path, count);
    }

    public void Dispose()
    {
        _writer.Dispose();
        _stream.Dispose();
    }

    private static IEnumerable<object[]> ReadRows(string path, long count)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, BufferSize, FileOptions.SequentialScan);
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8);

        for (var index = 0L; index < count; index++)
        {
            yield return DataRowSerializer.Read(reader);
        }
    }
}
