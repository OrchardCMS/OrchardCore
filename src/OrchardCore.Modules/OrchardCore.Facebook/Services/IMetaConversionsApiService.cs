using OrchardCore.Facebook.Models;
using OrchardCore.Infrastructure;

namespace OrchardCore.Facebook.Services;

/// <summary>
/// Sends server-side events to the Meta Conversions API for the configured pixel.
/// See https://developers.facebook.com/docs/marketing-api/conversions-api.
/// </summary>
public interface IMetaConversionsApiService
{
    /// <summary>
    /// Sends a single event. Returns a failed <see cref="Result"/> when the Meta Pixel or
    /// Conversions API settings are missing/invalid, or when the Graph API request fails.
    /// </summary>
    Task<Result> SendEventAsync(MetaConversionEvent conversionEvent, CancellationToken cancellationToken = default);
}
