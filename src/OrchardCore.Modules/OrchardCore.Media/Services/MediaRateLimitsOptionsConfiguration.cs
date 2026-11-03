using Microsoft.Extensions.Options;
using OrchardCore.RateLimits;

namespace OrchardCore.Media.Services;

/// <summary>
/// Excludes the media files from the global rate-limit policies.
/// </summary>
/// <remarks>
/// The media files are served after the rate limiter, unlike the other static files, so without this exclusion
/// each image of a page would consume the request budget of the visitor.
/// </remarks>
public sealed class MediaRateLimitsOptionsConfiguration : IConfigureOptions<RateLimitsOptions>
{
    private readonly MediaOptions _mediaOptions;

    public MediaRateLimitsOptionsConfiguration(IOptions<MediaOptions> mediaOptions)
    {
        _mediaOptions = mediaOptions.Value;
    }

    public void Configure(RateLimitsOptions options)
    {
        if (_mediaOptions.AssetsRequestPath.HasValue)
        {
            options.ExcludePathFromGlobalPolicies(_mediaOptions.AssetsRequestPath);
        }
    }
}
