namespace OrchardCore.Media
{
    public interface IMediaNameNormalizerService
    {
        public string NormalizeFolderName(string folderName);
        public string NormalizeFileName(string fileName);
    }
}
