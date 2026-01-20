namespace OrchardCore.Media;

public interface IMediaNameNormalizerService
{
    string NormalizeFolderName(string folderName);
    string NormalizeFileName(string fileName);
}
