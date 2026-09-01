using System.Buffers;
using System.Globalization;
using System.Text;

namespace OrchardCore.Media.Core;

/// <summary>
/// Maps media file store paths to names that are valid on the local file system, so remote
/// stores (e.g. Azure Blob Storage, Amazon S3) whose paths may contain characters or names
/// that are invalid on Windows/NTFS (e.g. ':', '*', trailing dots, reserved device names)
/// can still be mirrored inside the local media cache.
/// Offending characters are escaped as '%XX' (invariant uppercase hex). The mapping is
/// deterministic, collision free and reversible, and is applied on every platform so the
/// cache layout does not depend on the operating system.
/// </summary>
public static class MediaCachePathEscaper
{
    private const char EscapeCharacter = '%';

    // Characters that are invalid in NTFS/FAT file names (except '/', which is the path
    // delimiter), plus the escape character itself. Control characters are escaped too.
    private static readonly SearchValues<char> s_escapedCharacters = SearchValues.Create("\"*:<>?\\|%");

    // Windows reserved device names, invalid as a file or directory name, alone or with any extension.
    private static readonly string[] s_reservedNames =
    [
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
    ];

    /// <summary>
    /// Escapes a '/'-delimited media path so that every segment is a valid file system name.
    /// Returns the original instance when nothing needs escaping, which is the common case.
    /// </summary>
    public static string Escape(string path)
    {
        if (string.IsNullOrEmpty(path) || !NeedsEscaping(path))
        {
            return path;
        }

        var builder = new StringBuilder(path.Length + 8);
        var start = 0;
        while (true)
        {
            var separatorIndex = path.IndexOf('/', start);
            var end = separatorIndex >= 0 ? separatorIndex : path.Length;
            AppendEscapedSegment(builder, path.AsSpan(start, end - start));
            if (separatorIndex < 0)
            {
                return builder.ToString();
            }

            builder.Append('/');
            start = separatorIndex + 1;
        }
    }

    /// <summary>
    /// Reverses <see cref="Escape"/>. A '%' that is not followed by two hex digits is kept as-is,
    /// so paths that were never escaped unescape to themselves.
    /// </summary>
    public static string Unescape(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        var index = path.IndexOf(EscapeCharacter);
        if (index < 0)
        {
            return path;
        }

        var builder = new StringBuilder(path.Length);
        var lastCopied = 0;
        while (index >= 0)
        {
            builder.Append(path, lastCopied, index - lastCopied);
            if (index + 2 < path.Length && Uri.IsHexDigit(path[index + 1]) && Uri.IsHexDigit(path[index + 2]))
            {
                builder.Append((char)((Uri.FromHex(path[index + 1]) << 4) | Uri.FromHex(path[index + 2])));
                lastCopied = index + 3;
            }
            else
            {
                builder.Append(EscapeCharacter);
                lastCopied = index + 1;
            }

            index = path.IndexOf(EscapeCharacter, lastCopied);
        }

        builder.Append(path, lastCopied, path.Length - lastCopied);

        return builder.ToString();
    }

    private static bool NeedsEscaping(string path)
    {
        var start = 0;
        while (true)
        {
            var separatorIndex = path.IndexOf('/', start);
            var end = separatorIndex >= 0 ? separatorIndex : path.Length;
            if (SegmentNeedsEscaping(path.AsSpan(start, end - start)))
            {
                return true;
            }

            if (separatorIndex < 0)
            {
                return false;
            }

            start = separatorIndex + 1;
        }
    }

    private static bool SegmentNeedsEscaping(ReadOnlySpan<char> segment)
    {
        if (segment.IsEmpty)
        {
            return false;
        }

        if (segment.IndexOfAny(s_escapedCharacters) >= 0)
        {
            return true;
        }

        foreach (var c in segment)
        {
            if (c < ' ')
            {
                return true;
            }
        }

        // Windows strips trailing dots and spaces, which would make the cached name
        // differ from the requested one.
        var last = segment[^1];
        if (last == '.' || last == ' ')
        {
            return true;
        }

        return IsReservedName(segment);
    }

    private static void AppendEscapedSegment(StringBuilder builder, ReadOnlySpan<char> segment)
    {
        if (segment.IsEmpty)
        {
            return;
        }

        // Escape the whole trailing run of dots and spaces, Windows strips them all.
        var trailingStart = segment.Length;
        while (trailingStart > 0 && (segment[trailingStart - 1] == '.' || segment[trailingStart - 1] == ' '))
        {
            trailingStart--;
        }

        // Escaping the first character is enough to void a reserved device name.
        var escapeFirst = IsReservedName(segment);

        for (var i = 0; i < segment.Length; i++)
        {
            var c = segment[i];
            if (c < ' ' || s_escapedCharacters.Contains(c) || i >= trailingStart || (i == 0 && escapeFirst))
            {
                builder.Append(EscapeCharacter);
                builder.Append(((int)c).ToString("X2", CultureInfo.InvariantCulture));
            }
            else
            {
                builder.Append(c);
            }
        }
    }

    private static bool IsReservedName(ReadOnlySpan<char> segment)
    {
        // A reserved name is reserved with any extension too, e.g. 'con.txt'.
        var dotIndex = segment.IndexOf('.');
        var name = (dotIndex >= 0 ? segment[..dotIndex] : segment).TrimEnd(' ');
        if (name.Length is < 3 or > 4)
        {
            return false;
        }

        foreach (var reserved in s_reservedNames)
        {
            if (name.Equals(reserved, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
