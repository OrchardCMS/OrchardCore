using System.Security.Cryptography;
using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace OrchardCore.Build.Tasks;

public class CopyTranslationFilesTask : MSBuildTask
{
    public string SourceFile { get; set; }

    [Required]
    public string DestinationFolder { get; set; }

    public override bool Execute()
    {
        if (string.IsNullOrEmpty(SourceFile))
        {
            return true;
        }

        if (!Directory.Exists(DestinationFolder))
        {
            Directory.CreateDirectory(DestinationFolder);
        }

        var fileInfo = new FileInfo(SourceFile);
        var destinationFilePath = Path.Combine(DestinationFolder, fileInfo.Name);

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
