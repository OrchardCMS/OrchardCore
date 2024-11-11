using Microsoft.IO;

namespace OrchardCore;

public static class MemoryStreamFactory
{
    private static readonly RecyclableMemoryStreamManager _manager = new();

    static MemoryStreamFactory()
    {
        var options = new RecyclableMemoryStreamManager.Options
        {
            BlockSize = 4 * 1024, // 4 KB
            AggressiveBufferReturn = true
        };

        _manager = new RecyclableMemoryStreamManager(options);
    }

    public static RecyclableMemoryStream GetStream(string tag = null)
        => _manager.GetStream(tag);

    public static RecyclableMemoryStream GetStream(int requiredSize, string tag = null)
        => _manager.GetStream(tag, requiredSize);
}
