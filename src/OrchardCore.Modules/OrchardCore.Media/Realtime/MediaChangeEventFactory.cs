using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using OrchardCore.Media.Endpoints.Api;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Realtime;

/// <summary>
/// Builds the payload broadcast for a media change.
/// <para>
/// Historically the payload carried only the action and path, and every receiving client answered with a
/// full <c>GetDirectoryContent</c> refetch — so a single change cost one directory listing (plus a
/// <c>HasChildren</c> probe per subfolder) per connected client. Including the affected entry lets the
/// client patch its store instead, turning that fan-out into a single server-side lookup.
/// </para>
/// <para>
/// The entry is shaped by <see cref="MediaEndpointHelpers.CreateFileResult"/>, the same helper the
/// <c>GetDirectoryContent</c> endpoint uses, so clients can splice it straight into their list without
/// having to reconcile two shapes.
/// </para>
/// </summary>
public sealed class MediaChangeEventFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly IFileVersionProvider _fileVersionProvider;
    private readonly IMediaFileStore _mediaFileStore;

    public MediaChangeEventFactory(
        IHttpContextAccessor httpContextAccessor,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        IMediaFileStore mediaFileStore)
    {
        _httpContextAccessor = httpContextAccessor;
        _contentTypeProvider = contentTypeProvider;
        _fileVersionProvider = fileVersionProvider;
        _mediaFileStore = mediaFileStore;
    }

    /// <summary>
    /// Builds a payload for an event that affects a single path, such as a file upload or a deletion.
    /// </summary>
    public Task<MediaChangedEvent> CreateAsync(string action, string path, bool includeItem)
        => CreateAsync(action, path, newPath: null, includeItem);

    /// <summary>
    /// Builds a payload for an event that moves or copies a file, where <paramref name="newPath"/> is
    /// the entry the client should add.
    /// </summary>
    public async Task<MediaChangedEvent> CreateAsync(string action, string path, string newPath, bool includeItem)
    {
        var result = new MediaChangedEvent
        {
            Action = action,
            Path = path,
            NewPath = newPath,
        };

        if (includeItem)
        {
            // Null when the file is already gone, or when there is no ambient request to resolve the
            // public URL against. Clients treat a missing item as "refetch", so this degrades to the
            // previous behaviour rather than to a broken view.
            result.Item = await CreateItemAsync(newPath ?? path);
        }

        return result;
    }

    private async Task<FileStoreEntryDto> CreateItemAsync(string path)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // The public URL is built from the request's PathBase, which carries the tenant prefix. Guessing
        // it outside a request would produce broken URLs on prefixed tenants, so omit the entry instead.
        if (httpContext is null || string.IsNullOrEmpty(path))
        {
            return null;
        }

        var entry = await _mediaFileStore.GetFileInfoAsync(path);

        if (entry is null)
        {
            return null;
        }

        return MediaEndpointHelpers.CreateFileResult(
            entry,
            httpContext,
            _contentTypeProvider,
            _fileVersionProvider,
            _mediaFileStore);
    }
}
