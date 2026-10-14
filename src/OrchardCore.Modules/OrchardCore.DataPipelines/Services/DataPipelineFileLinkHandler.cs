using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DataPipelines.SharedLinks;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Environment.Shell;
using IIdGenerator = OrchardCore.Entities.IIdGenerator;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Shares the files of runs through shared links: keeps a copy of each file in the tenant's App_Data folder, out of the
/// media library, serves it to the recipients of its link, and deletes it with the link.
/// </summary>
public sealed class DataPipelineFileLinkHandler : ISharedLinkHandler
{
    /// <summary>
    /// The kind of the links to the files of runs.
    /// </summary>
    public const string LinkKind = "DataPipelines.File";

    /// <summary>
    /// The property of a link that holds the media type of its file.
    /// </summary>
    public const string ContentTypeProperty = "ContentType";

    /// <summary>
    /// The property of a link that holds the identifier of the run that shared its file.
    /// </summary>
    public const string RunIdProperty = "RunId";

    private readonly IOptions<ShellOptions> _shellOptions;
    private readonly ShellSettings _shellSettings;
    private readonly IIdGenerator _idGenerator;
    private readonly IStringLocalizer S;

    public DataPipelineFileLinkHandler(
        IOptions<ShellOptions> shellOptions,
        ShellSettings shellSettings,
        IIdGenerator idGenerator,
        IStringLocalizer<DataPipelineFileLinkHandler> localizer)
    {
        _shellOptions = shellOptions;
        _shellSettings = shellSettings;
        _idGenerator = idGenerator;
        S = localizer;
    }

    public string Kind => LinkKind;

    public LocalizedString DisplayName => S["Pipeline file"];

    /// <summary>
    /// Keeps a copy of a file, to share it.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The identifier of the copy, the <see cref="SharedLink.ResourceId"/> of its link.</returns>
    public async Task<string> StoreAsync(DataPipelineFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        var resourceId = _idGenerator.GenerateUniqueId();
        var directory = GetDirectory(resourceId);
        Directory.CreateDirectory(directory);

        await using var source = file.OpenRead();
        await using var target = new FileStream(Path.Combine(directory, Path.GetFileName(file.FileName)), FileMode.CreateNew, FileAccess.Write, FileShare.None, 16 * 1024, FileOptions.Asynchronous);
        await source.CopyToAsync(target, cancellationToken);

        return resourceId;
    }

    public Task<IActionResult> OpenAsync(SharedLink link, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(link);

        var path = Path.Combine(GetDirectory(link.ResourceId), Path.GetFileName(link.Title ?? string.Empty));

        if (!File.Exists(path))
        {
            return Task.FromResult<IActionResult>(new NotFoundResult());
        }

        var contentType = link.Properties.TryGetValue(ContentTypeProperty, out var value) && !string.IsNullOrEmpty(value) ? value : "application/octet-stream";
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 16 * 1024, FileOptions.Asynchronous);

        return Task.FromResult<IActionResult>(new FileStreamResult(stream, contentType) { FileDownloadName = link.Title });
    }

    public string GetSourceUrl(SharedLink link, IUrlHelper urlHelper)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentNullException.ThrowIfNull(urlHelper);

        return link.Properties.TryGetValue(RunIdProperty, out var runId) && !string.IsNullOrEmpty(runId)
            ? urlHelper.Action("Run", "Admin", new { area = "OrchardCore.DataPipelines", runId })
            : null;
    }

    public Task DeletedAsync(SharedLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        var directory = GetDirectory(link.ResourceId);

        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        return Task.CompletedTask;
    }

    private string GetDirectory(string resourceId)
    {
        return Path.Combine(_shellOptions.Value.ShellsApplicationDataPath, _shellOptions.Value.ShellsContainerName, _shellSettings.Name, "DataPipelines", "SharedFiles", Path.GetFileName(resourceId ?? string.Empty));
    }
}
