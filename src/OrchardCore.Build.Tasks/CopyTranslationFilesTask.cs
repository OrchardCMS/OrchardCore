using System.Security.Cryptography;
using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace OrchardCore.Build.Tasks;

public class CopyTranslationFilesTask : MSBuildTask
{
    public string SourceFilePath { get; set; }

    [Required]
    public string DestinationFolderPath { get; set; }

    public override bool Execute()
    {
        if (string.IsNullOrEmpty(SourceFilePath))
        {
            return true;
        }

        if (!Directory.Exists(DestinationFolderPath))
        {
            Directory.CreateDirectory(DestinationFolderPath);
        }

        var fileInfo = new FileInfo(SourceFilePath);
        var destinationFilePath = Path.Combine(DestinationFolderPath, fileInfo.Name);

        if (File.Exists(destinationFilePath))
        {
            // Skip unchanged files
            var sourceFileHash = GetFileHashAsync(fileInfo.FullName);
            var destinationFileHash = GetFileHashAsync(destinationFilePath);

            if (sourceFileHash.Equals(destinationFileHash, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        fileInfo.CopyTo(destinationFilePath, overwrite: true);

        return true;
    }

    private static string GetFileHashAsync(string fileName)
    {
        using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        var hash = SHA256.HashData(stream);

        return Convert.ToBase64String(hash);
    }
}
