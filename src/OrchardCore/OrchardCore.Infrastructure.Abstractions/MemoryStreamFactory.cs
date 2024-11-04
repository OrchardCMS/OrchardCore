using Microsoft.IO;

namespace OrchardCore;

public static class MemoryStreamFactory
{
    private static readonly RecyclableMemoryStreamManager _manager = new();

    public static MemoryStream GetStream(string tag = null)
        => _manager.GetStream(tag);
}
