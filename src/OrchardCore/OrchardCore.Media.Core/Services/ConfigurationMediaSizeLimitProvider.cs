using Microsoft.Extensions.Options;

namespace OrchardCore.Media.Services;

public class ConfigurationMediaSizeLimitProvider : IMediaSizeLimitProvider
{
    private readonly MediaOptions _mediaOptions;

    public int Order => int.MaxValue;

    public ConfigurationMediaSizeLimitProvider(IOptions<MediaOptions> options)
    {
        _mediaOptions = options.Value;
    }

    public Task<long?> GetMediaSizeLimitAsync() => Task.FromResult<long?>(_mediaOptions.MaxFileSize);
}
