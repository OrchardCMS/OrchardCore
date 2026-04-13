using Microsoft.Extensions.Options;
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

    public string NormalizeFolderName(string folderName)
    {
        return _slugService.Slugify(folderName, _options.Transliterate);
    }

    public string NormalizeFileName(string fileName)
    {
        return _slugService.Slugify(Path.GetFileNameWithoutExtension(fileName), _options.Transliterate) + Path.GetExtension(fileName);
    }
}
