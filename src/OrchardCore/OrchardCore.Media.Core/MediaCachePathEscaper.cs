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
    private const string CompactEscapePrefix = "%~";
    private const int MaximumComponentLength = 255;

    // Characters that are invalid in NTFS/FAT file names (except '/', which is the path
    // delimiter), plus the escape character itself. Control characters are escaped too.
    private static readonly SearchValues<char> s_escapedCharacters = SearchValues.Create("\"*:<>?\\|%");

    // Windows reserved device names, invalid as a file or directory name, alone or with any extension.
    private static readonly string[] s_reservedNames =
    [
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        "COM¹", "COM²", "COM³", "LPT¹", "LPT²", "LPT³",
    ];

    /// <summary>
    /// Escapes a '/'-delimited media path so that every segment is a valid file system name.
    /// Returns the original instance when nothing needs escaping, which is the common case.
    /// </summary>
    public static string Escape(string path)
        => Escape(path, preserveGlobPatterns: false);

    /// <summary>
    /// Escapes a file-provider glob pattern while retaining glob wildcards.
    /// </summary>
    public static string EscapeGlob(string pattern)
        => Escape(pattern, preserveGlobPatterns: true);

    private static string Escape(string path, bool preserveGlobPatterns)
    {
        if (string.IsNullOrEmpty(path) || !NeedsEscaping(path, preserveGlobPatterns))
        {
            return path;
        }

        var builder = new StringBuilder(path.Length + 8);
        var start = 0;
        while (true)
        {
            var separatorIndex = path.IndexOf('/', start);
            var end = separatorIndex >= 0 ? separatorIndex : path.Length;
            AppendEscapedSegment(builder, path.AsSpan(start, end - start), preserveGlobPatterns);
            if (separatorIndex < 0)
            {
                return builder.ToString();
            }

            builder.Append('/');
            start = separatorIndex + 1;
        }
    }

    /// <summary>
    /// Reverses <see cref="Escape(string)"/>. A '%' that is not followed by two hex digits is kept as-is,
    /// so paths that were never escaped unescape to themselves.
    /// </summary>
    public static string Unescape(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        if (path.IndexOf(EscapeCharacter) < 0)
        {
            return path;
        }

        var builder = new StringBuilder(path.Length);
        var start = 0;
        while (true)
        {
            var separatorIndex = path.IndexOf('/', start);
            var end = separatorIndex >= 0 ? separatorIndex : path.Length;
            var segment = path.AsSpan(start, end - start);
            if (TryUnescapeCompactSegment(segment, out var unescaped))
            {
                builder.Append(unescaped);
            }
            else
            {
                AppendUnescapedSegment(builder, segment);
            }

            if (separatorIndex < 0)
            {
                return builder.ToString();
            }

            builder.Append('/');
            start = separatorIndex + 1;
        }
    }

    private static bool NeedsEscaping(string path, bool preserveGlobPatterns)
    {
        var start = 0;
        while (true)
        {
            var separatorIndex = path.IndexOf('/', start);
            var end = separatorIndex >= 0 ? separatorIndex : path.Length;
            if (SegmentNeedsEscaping(path.AsSpan(start, end - start), preserveGlobPatterns))
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

    private static bool SegmentNeedsEscaping(ReadOnlySpan<char> segment, bool preserveGlobPatterns)
    {
        if (segment.IsEmpty)
        {
            return false;
        }

        foreach (var c in segment)
        {
            if (s_escapedCharacters.Contains(c) && (!preserveGlobPatterns || (c != '*' && c != '?')))
            {
                return true;
            }
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

    private static void AppendEscapedSegment(StringBuilder builder, ReadOnlySpan<char> segment, bool preserveGlobPatterns)
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

        var escaped = new StringBuilder(segment.Length + 8);

        // Escaping the first character is enough to void a reserved device name.
        var escapeFirst = IsReservedName(segment);

        for (var i = 0; i < segment.Length; i++)
        {
            var c = segment[i];
            if (c < ' ' ||
                (s_escapedCharacters.Contains(c) && (!preserveGlobPatterns || (c != '*' && c != '?'))) ||
                i >= trailingStart ||
                (i == 0 && escapeFirst))
            {
                escaped.Append(EscapeCharacter);
                escaped.Append(((int)c).ToString("X2", CultureInfo.InvariantCulture));
            }
            else
            {
                escaped.Append(c);
            }
        }

        if (escaped.Length > MaximumComponentLength)
        {
            builder.Append(CompactEscapePrefix);
            builder.Append(segment.Length.ToString(CultureInfo.InvariantCulture));
            builder.Append(EscapeCharacter);
            builder.Append(ToBase64Url(Encoding.UTF8.GetBytes(segment.ToString())));
        }
        else
        {
            builder.Append(escaped);
        }
    }

    private static void AppendUnescapedSegment(StringBuilder builder, ReadOnlySpan<char> segment)
    {
        var index = segment.IndexOf(EscapeCharacter);
        var lastCopied = 0;
        while (index >= 0)
        {
            builder.Append(segment[lastCopied..index]);
            if (index + 2 < segment.Length && Uri.IsHexDigit(segment[index + 1]) && Uri.IsHexDigit(segment[index + 2]))
            {
                builder.Append((char)((Uri.FromHex(segment[index + 1]) << 4) | Uri.FromHex(segment[index + 2])));
                lastCopied = index + 3;
            }
            else
            {
                builder.Append(EscapeCharacter);
                lastCopied = index + 1;
            }

            index = segment[lastCopied..].IndexOf(EscapeCharacter);
            if (index >= 0)
            {
                index += lastCopied;
            }
        }

        builder.Append(segment[lastCopied..]);
    }

    private static string ToBase64Url(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static bool TryUnescapeCompactSegment(ReadOnlySpan<char> segment, out string value)
    {
        try
        {
            value = null;
            if (!segment.StartsWith(CompactEscapePrefix, StringComparison.Ordinal))
            {
                return false;
            }

            var separatorIndex = segment[CompactEscapePrefix.Length..].IndexOf(EscapeCharacter);
            if (separatorIndex < 1 ||
                !int.TryParse(segment.Slice(CompactEscapePrefix.Length, separatorIndex), CultureInfo.InvariantCulture, out var length))
            {
                return false;
            }

            var base64 = segment[(CompactEscapePrefix.Length + separatorIndex + 1)..].ToString().Replace('-', '+').Replace('_', '/');
            var bytes = Convert.FromBase64String(base64.PadRight(base64.Length + ((4 - base64.Length % 4) % 4), '='));
            value = new UTF8Encoding(false, true).GetString(bytes);
            return value.Length == length;
        }
        catch (FormatException)
        {
            value = null;
            return false;
        }
        catch (DecoderFallbackException)
        {
            value = null;
            return false;
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
