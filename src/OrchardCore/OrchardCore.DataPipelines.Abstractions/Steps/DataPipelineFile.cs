namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// A file produced during a run, such as an export, stored in the run's temporary folder until the run ends.
/// Destinations read it to deliver it.
/// </summary>
public sealed class DataPipelineFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataPipelineFile"/> class.
    /// </summary>
    /// <param name="fileName">The name of the file, as delivered.</param>
    /// <param name="contentType">The media type of the file.</param>
    /// <param name="path">The full path of the temporary file that holds the content.</param>
    public DataPipelineFile(string fileName, string contentType, string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        ArgumentException.ThrowIfNullOrEmpty(path);

        FileName = fileName;
        ContentType = string.IsNullOrEmpty(contentType) ? "application/octet-stream" : contentType;
        Path = path;
    }

    /// <summary>
    /// Gets the name of the file, as delivered.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the media type of the file.
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Gets the full path of the temporary file that holds the content.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets or sets the number of rows the file holds, when it was created from rows.
    /// </summary>
    public long? RowCount { get; set; }

    /// <summary>
    /// Gets the size of the file, in bytes.
    /// </summary>
    public long Length => File.Exists(Path) ? new FileInfo(Path).Length : 0;

    /// <summary>
    /// Opens the content of the file for reading.
    /// </summary>
    /// <returns>A read-only stream. The caller disposes it.</returns>
    public Stream OpenRead()
        => new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read, 16 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
}
