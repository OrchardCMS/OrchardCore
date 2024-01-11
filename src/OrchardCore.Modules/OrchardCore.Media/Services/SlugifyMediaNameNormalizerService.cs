using System.IO;
using OrchardCore.Modules.Services;

namespace OrchardCore.Media.Services
{
    public class SlugifyMediaNameNormalizerService : IMediaNameNormalizerService
    {
        private readonly ISlugService _slugService;

        public SlugifyMediaNameNormalizerService(ISlugService slugService)
        {
            _slugService = slugService;
        }

        public string NormalizeFolderName(string folderName)
        {
            return _slugService.Slugify(folderName);
        }

        public string NormalizeFileName(string fileName)
        {
            return _slugService.Slugify(Path.GetFileNameWithoutExtension(fileName)) + Path.GetExtension(fileName);
        }
    }
}
