namespace OrchardCore.Modules.FileProviders;

/// <summary>
/// Resolves physical paths that are requested through a virtual path, while ensuring that the
/// resolved path is contained in the root folder that the virtual path is mapped to.
/// </summary>
public static class PhysicalPathResolver
{
    private static readonly StringComparison s_pathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    /// <summary>
    /// Canonicalizes a root folder path, with a trailing directory separator, so that it can be
    /// used with <see cref="TryResolve(string, string, out string)"/>.
    /// </summary>
    public static string NormalizeRoot(string root)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;

    /// <summary>
    /// Resolves a relative path against a root folder normalized by <see cref="NormalizeRoot(string)"/>,
    /// but only if the canonical resolved path is still contained in that root folder.
    /// </summary>
    /// <remarks>
    /// A relative path may come from a request, where a '..' segment or a path alias of the current
    /// platform can be used to escape the root folder.
    /// </remarks>
    public static bool TryResolve(string root, string relativePath, out string physicalPath)
    {
        physicalPath = null;

        string resolvedPath;

        try
        {
            resolvedPath = Path.GetFullPath(root + relativePath);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or IOException)
        {
            // The path can't be resolved to a physical path, e.g. it contains a volume separator.
            return false;
        }

        if (!resolvedPath.StartsWith(root, s_pathComparison))
        {
            return false;
        }

        physicalPath = resolvedPath;

        return true;
    }
}
