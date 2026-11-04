using OrchardCore.Documents;
using OrchardCore.Media.Models;

namespace OrchardCore.Media.Services;

public class MediaProfilesManager
{
    private readonly IDocumentManager<MediaProfilesDocument> _documentManager;

    public MediaProfilesManager(IDocumentManager<MediaProfilesDocument> documentManager) => _documentManager = documentManager;

    /// <summary>
    /// Loads the media profiles document from the store for updating and that should not be cached.
    /// </summary>
    public Task<MediaProfilesDocument> LoadMediaProfilesDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the media profiles document from the cache for sharing and that should not be updated.
    /// </summary>
    public Task<MediaProfilesDocument> GetMediaProfilesDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();

    public async Task RemoveMediaProfileAsync(string name)
    {
        var document = await LoadMediaProfilesDocumentAsync();
        document.MediaProfiles.Remove(name);
        await _documentManager.UpdateAsync(document);
    }

    /// <summary>Creates or replaces a profile using an invariant lowercase storage key.</summary>
    public Task UpdateMediaProfileAsync(string name, MediaProfile mediaProfile) => SaveMediaProfileAsync(null, name, mediaProfile);

    internal async Task SaveMediaProfileAsync(string sourceName, string name, MediaProfile mediaProfile)
    {
        var document = await LoadMediaProfilesDocumentAsync();
        if (sourceName is not null && !string.Equals(sourceName, name, StringComparison.OrdinalIgnoreCase))
        {
            document.MediaProfiles.Remove(sourceName);
        }
        document.MediaProfiles[name.ToLowerInvariant()] = mediaProfile;
        await _documentManager.UpdateAsync(document);
    }
}
