using Microsoft.Extensions.Options;
using OrchardCore.Localization;
using OrchardCore.Modules.Services;

namespace OrchardCore.Media.Services;

public class SlugifyMediaNameNormalizerService : IMediaNameNormalizerService
{
    private readonly ISlugService _slugService;
    private readonly MediaSlugifyOptions _options;

    public SlugifyMediaNameNormalizerService(
        ISlugService slugService,
        IOptions<MediaSlugifyOptions> options)
    {
        _slugService = slugService;
        _options = options.Value;
    }

    public string NormalizeFolderName(string folderName) =>
        Slugify(folderName);

    public string NormalizeFileName(string fileName) =>
        Slugify(Path.GetFileNameWithoutExtension(fileName)) + Path.GetExtension(fileName);

    private string Slugify(string name) =>
        _options.Transliterate
            ? _slugService.SlugifyAndTransliterate(name)
            : _slugService.Slugify(name);
}
