using System.Collections.Concurrent;
using System.Security.Claims;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Describes one run of a pipeline to its steps: who it runs for, its parameters, its temporary folder, and what it
/// delivered.
/// </summary>
public sealed class DataPipelineRunContext
{
    private readonly ConcurrentQueue<DataPipelineDelivery> _deliveries = new();
    private int _fileCount;

    /// <summary>
    /// The number of rows each source reads, by default, when a pipeline is previewed.
    /// </summary>
    public const int DefaultPreviewRowLimit = 100;

    /// <summary>
    /// Gets or sets the identifier of the run.
    /// </summary>
    public string RunId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the pipeline.
    /// </summary>
    public string PipelineId { get; set; }

    /// <summary>
    /// Gets or sets the name of the pipeline.
    /// </summary>
    public string PipelineName { get; set; }

    /// <summary>
    /// Gets or sets the number of the version of the pipeline that runs, or <c>0</c> for a draft.
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Gets or sets the user the run reads and writes data for. A published pipeline runs for the user who published
    /// it, who vouches for what it reads and writes; a preview runs for the designer.
    /// </summary>
    public ClaimsPrincipal User { get; set; }

    /// <summary>
    /// Gets or sets the services of the scope the run executes in.
    /// </summary>
    public IServiceProvider Services { get; set; }

    /// <summary>
    /// Gets or sets the values passed to the run, such as the input of the workflow that started it.
    /// </summary>
    public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets a value indicating whether the run previews the pipeline: sources read a few rows only, and
    /// destinations are not executed.
    /// </summary>
    public bool IsPreview { get; set; }

    /// <summary>
    /// Gets or sets the number of rows each source reads when the run is a preview.
    /// </summary>
    public int PreviewRowLimit { get; set; } = DefaultPreviewRowLimit;

    /// <summary>
    /// Gets or sets when the run started, in UTC.
    /// </summary>
    public DateTime StartedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the folder the files of the run are created in. It is deleted when the run ends.
    /// </summary>
    public string TemporaryDirectory { get; set; }

    /// <summary>
    /// Gets or sets the token that is cancelled when the run is cancelled or fails.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Gets or sets how each step gets the services it runs with. Steps run at the same time, so a host that uses
    /// services that aren't thread-safe, such as a database session, gives each step its own scope: the delegate
    /// receives the step, and the work to do with the services of its scope. When it is not set, every step uses
    /// <see cref="Services"/>.
    /// </summary>
    public Func<Models.DataPipelineStep, Func<IServiceProvider, Task>, Task> StepScope { get; set; }

    /// <summary>
    /// Gets what the run delivered so far, such as the files it saved or sent.
    /// </summary>
    public IReadOnlyCollection<DataPipelineDelivery> Deliveries => _deliveries;

    /// <summary>
    /// Records something the run delivered.
    /// </summary>
    /// <param name="delivery">The delivery.</param>
    public void AddDelivery(DataPipelineDelivery delivery)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        _deliveries.Enqueue(delivery);
    }

    /// <summary>
    /// Creates an empty file in the temporary folder of the run. Write its content with <see cref="File.Create(string)"/>
    /// on <see cref="DataPipelineFile.Path"/>.
    /// </summary>
    /// <param name="fileName">The name of the file, as delivered. Folders are removed.</param>
    /// <param name="contentType">The media type of the file.</param>
    /// <returns>The file.</returns>
    public DataPipelineFile CreateFile(string fileName, string contentType)
    {
        if (string.IsNullOrEmpty(TemporaryDirectory))
        {
            throw new InvalidOperationException("The run has no temporary folder.");
        }

        var name = Path.GetFileName(fileName ?? string.Empty);

        if (string.IsNullOrWhiteSpace(name))
        {
            name = "file";
        }

        // Each file gets its own folder, so files with the same name don't collide.
        var folder = Path.Combine(TemporaryDirectory, Interlocked.Increment(ref _fileCount).ToString(System.Globalization.CultureInfo.InvariantCulture));
        Directory.CreateDirectory(folder);

        var path = Path.Combine(folder, name);

        using (File.Create(path))
        {
        }

        return new DataPipelineFile(name, contentType, path);
    }
}
