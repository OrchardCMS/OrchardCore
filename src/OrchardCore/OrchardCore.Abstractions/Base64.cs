using System.Text;

namespace OrchardCore;

public static class Base64
{
    /// <summary>
    /// Converts a base64 encoded UTF8 string to the original value.
    /// </summary>
    /// <param name="base64">The base64 encoded string.</param>
    /// <returns>The decoded string.</returns>
    /// <remarks>This method is equivalent to <c>Encoding.UTF8.GetString(Convert.FromBase64String(base64))</c> but uses a buffer pool to decode the string.</remarks>
    public static string FromUTF8Base64String(string base64)
    {
        ArgumentNullException.ThrowIfNull(base64);

        // Due to padding the deserialized buffer could be smaller than this value.
        var maxBufferLength = GetDeserializedBase64Length(base64.Length);

        using var memoryStream = MemoryStreamFactory.GetStream(maxBufferLength);
        var span = memoryStream.GetSpan(maxBufferLength);

        if (!Convert.TryFromBase64String(base64, span, out var bytesWritten))
        {
            throw new FormatException("Invalid Base64 string.");
        }

        return Encoding.UTF8.GetString(span.Slice(0, bytesWritten));
    }

    /// <summary>
    /// Converts a base64 encoded string to a stream.
    /// </summary>
    /// <param name="base64">The base64 encoded string.</param>
    /// <remarks>The resulting <see cref="Stream"/> should be disposed once used.</remarks>
    /// <returns>The decoded stream.</returns>
    /// <exception cref="FormatException"></exception>
    public static Stream DecodedToStream(string base64)
    {
        ArgumentNullException.ThrowIfNull(base64);

        // Due to padding the deserialized buffer could be smaller than this value.
        var maxBufferLength = GetDeserializedBase64Length(base64.Length);

        var memoryStream = MemoryStreamFactory.GetStream(maxBufferLength);
        var span = memoryStream.GetSpan(maxBufferLength);

        if (!Convert.TryFromBase64String(base64, span, out var bytesWritten))
        {
            throw new FormatException("Invalid Base64 string.");
        }

        memoryStream.Advance(bytesWritten);

        return memoryStream;
    }

    /// <summary>
    /// Gets the maximum buffer length required to decode a base64 string.
    /// </summary>
    /// <param name="base64Length">The length value to decode.</param>
    /// <returns>The size of the decoded buffer.</returns>
    public static int GetDeserializedBase64Length(int base64Length)
    {
        // Do the multiplication first to prevent precision loss.
        return base64Length * 3 / 4;
    }
}
