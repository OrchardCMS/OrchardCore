using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Localization.PortableObject;

/// <summary>
/// Represents a provider that provides a translations for .po files.
/// </summary>
public class PoFilesTranslationsProvider : ITranslationProvider
{
    private readonly ILocalizationFileLocationProvider _poFilesLocationProvider;

    /// <summary>
    /// Creates a new instance of <see cref="PoFilesTranslationsProvider"/>.
    /// </summary>
    /// <param name="poFileLocationProvider">The <see cref="ILocalizationFileLocationProvider"/>.</param>
    public PoFilesTranslationsProvider(ILocalizationFileLocationProvider poFileLocationProvider)
    {
        _poFilesLocationProvider = poFileLocationProvider;
    }

    /// <inheritdocs />
    public void LoadTranslations(string cultureName, CultureDictionary dictionary)
    {
        foreach (var fileInfo in _poFilesLocationProvider.GetLocations(cultureName))
        {
            LoadFileToDictionary(fileInfo, dictionary);
        }
    }

    private static void LoadFileToDictionary(IFileInfo fileInfo, CultureDictionary dictionary)
    {
        if (fileInfo.Exists && !fileInfo.IsDirectory)
        {
            using var stream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
            var culureDictionaryRecords = PoParser.ParseAsync(reader)
                .ToListAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();

            dictionary.MergeTranslations(culureDictionaryRecords);
        }
    }
}
