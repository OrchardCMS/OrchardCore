using Microsoft.IO;

namespace OrchardCore;

public static class MemoryStreamFactory
{
    private static readonly RecyclableMemoryStreamManager _manager = new();

    public static RecyclableMemoryStream GetStream(string tag = null)
        => _manager.GetStream(tag);

    public static RecyclableMemoryStream GetStream(int requiredSize, string tag = null)
        => _manager.GetStream(tag, requiredSize);
}
