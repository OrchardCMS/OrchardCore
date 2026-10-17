namespace OrchardCore.DataSources.Operations;

/// <summary>
/// How much memory an operation that must see many rows at once, such as a sort or a join, may use, and where it
/// writes the rows that don't fit. The operations keep their memory near the limit whatever the number of rows, and
/// delete their temporary files when they are done.
/// </summary>
public sealed class DataSpillOptions
{
    /// <summary>
    /// The memory an operation may use by default: 32 MB.
    /// </summary>
    public const long DefaultMemoryLimit = 32L * 1024 * 1024;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSpillOptions"/> class.
    /// </summary>
    /// <param name="temporaryDirectory">The directory of the temporary files, created when needed.</param>
    /// <param name="memoryLimit">The estimated number of bytes of rows the operation keeps in memory before it writes them to temporary files.</param>
    public DataSpillOptions(string temporaryDirectory, long memoryLimit = DefaultMemoryLimit)
    {
        ArgumentException.ThrowIfNullOrEmpty(temporaryDirectory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(memoryLimit);

        TemporaryDirectory = temporaryDirectory;
        MemoryLimit = memoryLimit;
    }

    /// <summary>
    /// Gets the directory of the temporary files, created when needed.
    /// </summary>
    public string TemporaryDirectory { get; }

    /// <summary>
    /// Gets the estimated number of bytes of rows the operation keeps in memory before it writes them to temporary
    /// files.
    /// </summary>
    public long MemoryLimit { get; }
}
