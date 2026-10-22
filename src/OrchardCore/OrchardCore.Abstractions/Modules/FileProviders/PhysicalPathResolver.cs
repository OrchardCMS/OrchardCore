namespace OrchardCore.Modules.FileProviders;

/// <summary>
/// Resolves physical paths that are requested through a virtual path, while ensuring that the
/// resolved path is contained in the root folder that the virtual path is mapped to.
/// </summary>
/// <remarks>
/// Containment is checked with an ordinal comparison on every platform. The operating system is not a
/// reliable proxy for the case sensitivity of the file system: macOS is case insensitive on a default
/// APFS volume but not on a case sensitive one, Windows supports per directory case sensitivity, and a
/// case insensitive volume can be mounted on Linux. Ignoring case is the permissive direction, and on a
/// case sensitive file system it would accept a sibling folder that differs from the root only by case.
/// An ordinal comparison is therefore the conservative choice everywhere, and it does not reject any
/// path that stays in the root folder, because the root is then a literal prefix of the resolved path.
/// </remarks>
public static class PhysicalPathResolver
{
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

        if (!resolvedPath.StartsWith(root, StringComparison.Ordinal))
        {
            return false;
        }

        physicalPath = resolvedPath;

        return true;
    }
}
