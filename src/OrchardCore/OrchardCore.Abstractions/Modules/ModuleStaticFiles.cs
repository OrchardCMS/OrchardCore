using Microsoft.Extensions.Primitives;

namespace OrchardCore.Modules;

internal static class ModuleStaticFiles
{
    private static readonly char[] s_pathSeparators = ['/', '\\'];

    /// <summary>
    /// Determines whether a normalized module subpath escapes its root through parent
    /// directory ('..') segments. This mirrors the guard used by
    /// <c>Microsoft.Extensions.FileProviders.Physical.PhysicalFileProvider</c> so that the
    /// module file providers, which resolve physical files by concatenating a subpath to a
    /// trusted root, cannot be used for path traversal. It matters because encoded separators
    /// (e.g. '%5c') can survive the server request path normalization and only become '..'
    /// traversal once backslashes are converted to forward slashes during resolution.
    /// </summary>
    public static bool NavigatesAboveRoot(string subpath)
    {
        var tokenizer = new StringTokenizer(subpath, s_pathSeparators);
        var depth = 0;

        foreach (var segment in tokenizer)
        {
            if (segment.Length == 0 || segment.Equals(".", StringComparison.Ordinal))
            {
                continue;
            }

            if (segment.Equals("..", StringComparison.Ordinal))
            {
                if (--depth < 0)
                {
                    return true;
                }
            }
            else
            {
                depth++;
            }
        }

        return false;
    }
}
