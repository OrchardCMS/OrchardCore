using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell;

namespace OrchardCore.DataOrchestrator.Services;

/// <summary>
/// Resolves Data Orchestrator local file paths under the tenant's App_Data folder.
/// </summary>
public static class EtlLocalFilePathResolver
{
    /// <summary>
    /// The tenant App_Data subfolder used by Data Orchestrator file-based activities.
    /// </summary>
    public const string DefaultFilesDirectory = "DataOrchestrator";

    /// <summary>
    /// The subfolder where uploaded files are stored.
    /// </summary>
    public const string DefaultUploadsDirectory = "Uploads";

    /// <summary>
    /// The supported Excel workbook file extension.
    /// </summary>
    public const string ExcelFileExtension = ".xlsx";

    private static readonly char[] _directorySeparators = ['\\', '/'];

    private static readonly char[] _invalidPathCharacters =
    [
        .. Path.GetInvalidPathChars(),
        '~',
        '{',
        '}',
    ];

    /// <summary>
    /// Gets the absolute tenant-local base path used by Data Orchestrator files.
    /// </summary>
    public static string GetFilesBasePath(ShellOptions shellOptions, ShellSettings shellSettings)
    {
        ArgumentNullException.ThrowIfNull(shellOptions);
        ArgumentNullException.ThrowIfNull(shellSettings);

        return Path.GetFullPath(Path.Combine(
            shellOptions.ShellsApplicationDataPath,
            "Sites",
            shellSettings.Name,
            DefaultFilesDirectory));
    }

    /// <summary>
    /// Returns whether a configured file path is tenant-relative and safe to resolve under the base folder.
    /// </summary>
    public static bool IsValidRelativeFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        filePath = filePath.Trim();

        if (filePath.IndexOfAny(_invalidPathCharacters) >= 0 ||
            filePath.Contains("{%", StringComparison.Ordinal))
        {
            return false;
        }

        if (ContainsNavigationSegments(filePath))
        {
            return false;
        }

        if (filePath.StartsWith('\\') ||
            filePath.StartsWith("//", StringComparison.Ordinal) ||
            filePath.StartsWith(@"\\", StringComparison.Ordinal) ||
            IsWindowsDriveQualifiedPath(filePath))
        {
            return false;
        }

        if (filePath[0] is not '/' && Path.IsPathFullyQualified(filePath))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns whether a file path uses the supported Excel workbook extension.
    /// </summary>
    public static bool IsSupportedExcelFilePath(string filePath)
    {
        return string.Equals(Path.GetExtension(filePath), ExcelFileExtension, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Normalizes a tenant-relative file path to the current platform separators.
    /// </summary>
    public static string NormalizeRelativeFilePath(string filePath)
    {
        return filePath
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar)
            .Trim()
            .TrimStart(Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Resolves a tenant-relative file path under the configured base folder.
    /// </summary>
    public static string ResolveFilePath(string basePath, string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);

        if (!IsValidRelativeFilePath(filePath))
        {
            throw new InvalidOperationException("The file path is invalid.");
        }

        var fullBasePath = Path.GetFullPath(basePath);
        var fullFilePath = Path.GetFullPath(Path.Combine(fullBasePath, NormalizeRelativeFilePath(filePath)));

        if (!IsWithinBasePath(fullBasePath, fullFilePath))
        {
            throw new InvalidOperationException($"The file path '{filePath}' resolves outside the configured file base '{fullBasePath}'.");
        }

        return fullFilePath;
    }

    /// <summary>
    /// Saves an uploaded Excel workbook under the configured base folder and returns its tenant-relative path.
    /// </summary>
    public static async Task<string> SaveUploadedExcelFileAsync(IFormFile file, string basePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Length == 0)
        {
            throw new InvalidOperationException("The uploaded file is empty.");
        }

        var fileName = Path.GetFileName(file.FileName);

        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            !IsSupportedExcelFilePath(fileName))
        {
            throw new InvalidOperationException("Only .xlsx files can be uploaded.");
        }

        var relativePath = Path.Combine(DefaultUploadsDirectory, fileName);
        var fullPath = ResolveFilePath(basePath, relativePath);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        return relativePath.Replace(Path.DirectorySeparatorChar, '/');
    }

    private static bool ContainsNavigationSegments(string filePath)
    {
        return filePath
            .Split(_directorySeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(segment => segment is "." or "..");
    }

    private static bool IsWindowsDriveQualifiedPath(string filePath)
    {
        return filePath.Length >= 3 &&
            char.IsAsciiLetter(filePath[0]) &&
            filePath[1] == ':' &&
            filePath[2] is '\\' or '/';
    }

    private static bool IsWithinBasePath(string basePath, string filePath)
    {
        var relativePath = Path.GetRelativePath(basePath, filePath);

        return relativePath == "." ||
            (!relativePath.Equals("..", StringComparison.Ordinal) &&
             !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
             !Path.IsPathRooted(relativePath));
    }
}
