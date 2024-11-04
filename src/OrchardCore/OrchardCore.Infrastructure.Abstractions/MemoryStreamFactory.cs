using Microsoft.IO;

namespace OrchardCore;

public static class MemoryStreamFactory
{
    private static readonly RecyclableMemoryStreamManager _manager = new();

    public static MemoryStream GetStream(byte[] buffer, int offset, int count, string tag = null)
        => _manager.GetStream(tag, buffer, offset, count);

    public static MemoryStream GetStream(byte[] buffer, string tag = null)
        => _manager.GetStream(tag, buffer, 0, buffer.Length);

    public static MemoryStream GetStream()
        => _manager.GetStream();

    public static MemoryStream GetStream(string tag)
        => _manager.GetStream(tag);
}
