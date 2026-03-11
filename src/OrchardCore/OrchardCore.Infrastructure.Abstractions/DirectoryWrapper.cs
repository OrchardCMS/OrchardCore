namespace OrchardCore.Infrastructure;

public static class DirectoryWrapper
{
    /// <summary>
    /// Recursively copies a directory and its contents to a new location.
    /// </summary>
    /// <param name="sourceDirectory">The source directory path.</param>
    /// <param name="destinationDirectory">The destination directory path.</param>
    /// <param name="overwrite">Whether to overwrite existing files.</param>
    public static void CopyDirectory(string sourceDirectory, string destinationDirectory, bool overwrite = true)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceDirectory);
        ArgumentException.ThrowIfNullOrEmpty(destinationDirectory);

        if (!Directory.Exists(sourceDirectory))
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory}.");
        }

        Directory.CreateDirectory(destinationDirectory);

        foreach (var filePath in Directory.GetFiles(sourceDirectory))
        {
            var fileName = Path.GetFileName(filePath);
            var destinationFilePath = Path.Combine(destinationDirectory, fileName);

            File.Copy(filePath, destinationFilePath, overwrite);
        }

        foreach (var subDirectory in Directory.GetDirectories(sourceDirectory))
        {
            var subDirectoryName = Path.GetFileName(subDirectory);
            var destinationSubDirectory = Path.Combine(destinationDirectory, subDirectoryName);

            CopyDirectory(subDirectory, destinationSubDirectory, overwrite);
        }
    }
}
