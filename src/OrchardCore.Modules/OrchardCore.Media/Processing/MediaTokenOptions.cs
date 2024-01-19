using Microsoft.Extensions.Options;

namespace OrchardCore.Media.Processing
{
    public class MediaTokenOptions : IAsyncOptions
    {
        public byte[] HashKey { get; set; }
    }
}
