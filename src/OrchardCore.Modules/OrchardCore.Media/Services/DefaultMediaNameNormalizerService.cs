namespace OrchardCore.Media.Services
{
    public class DefaultMediaNameNormalizerService : IMediaNameNormalizerService
    {
        public string NormalizeFolderName(string folderName) => folderName;

        public string NormalizeFileName(string fileName) => fileName;
    }
}
