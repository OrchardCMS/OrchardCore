using System.Text;

namespace OrchardCore;

public static class Str
{
    public static ReadOnlySpan<byte> FromBase64String(string base64)
    {
        if (string.IsNullOrEmpty(base64))
        {
            return [];
        }
        var neededLength = GetByteArrayLengthFromBase64(base64);

        using var memoryStream = MemoryStreamFactory.GetStream(neededLength);
        var bytes = memoryStream.GetSpan(neededLength);

        if (!Convert.TryFromBase64String(base64, bytes, out var _))
        {
            throw new FormatException("Invalid Base64 string.");
        }

        return bytes;
    }

    public static bool TryFromBase64String(string base64, out ReadOnlySpan<byte> bytes)
    {
        if (string.IsNullOrEmpty(base64))
        {
            bytes = [];

            return false;
        }

        Span<byte> data = new byte[GetByteArrayLengthFromBase64(base64)];

        if (!Convert.TryFromBase64String(base64, data, out var _))
        {
            bytes = [];

            return false;
        }

        bytes = data;

        return true;
    }

    // Base64 string length gives us the original byte length. It encodes 3 bytes into 4 characters.
    // The formula to calculate the number of bytes from the base64 string length is:
    private static int GetByteArrayLengthFromBase64(string base64String)
    {
        return (base64String.Length * 3) / 4;
    }
}
