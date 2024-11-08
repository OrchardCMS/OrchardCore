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

    // The length of a Base64-encoded string corresponds to the original byte length. 
    // Base64 encoding converts every 3 bytes of data into 4 characters. 
    // To calculate the original byte length from the Base64 string, use the formula: 
    // (length * 3) / 4. This ensures the correct byte count by multiplying the length by 3 
    // before dividing by 4.
    private static int GetByteArrayLengthFromBase64(string base64String)
    {
        return base64String.Length * 3 / 4;
    }
}
