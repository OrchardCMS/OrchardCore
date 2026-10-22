namespace OrchardCore.Facebook.Models;

/// <summary>
/// The value of Meta's <c>action_source</c> event field, describing where the conversion took place.
/// See https://developers.facebook.com/docs/marketing-api/conversions-api/parameters/action-source.
/// </summary>
public enum MetaActionSource
{
    Website,
    Email,
    App,
    PhoneCall,
    Chat,
    PhysicalStore,
    SystemGenerated,
    Other,
}

/// <summary>
/// A single Meta Conversions API event, as sent to the <c>/{pixel_id}/events</c> Graph API endpoint.
/// See https://developers.facebook.com/docs/marketing-api/conversions-api/using-the-api.
/// </summary>
public class MetaConversionEvent
{
    /// <summary>
    /// One of Meta's standard event names (e.g. <c>Purchase</c>, <c>Lead</c>) or a custom event name.
    /// </summary>
    public string EventName { get; set; }

    /// <summary>
    /// When the event occurred. Defaults to <see cref="DateTimeOffset.UtcNow"/> when unset.
    /// </summary>
    public DateTimeOffset? EventTime { get; set; }

    /// <summary>
    /// The browser URL where the event happened. Recommended for <see cref="MetaActionSource.Website"/> events.
    /// </summary>
    public string EventSourceUrl { get; set; }

    public MetaActionSource ActionSource { get; set; } = MetaActionSource.Website;

    /// <summary>
    /// A unique id for this event, used by Meta to deduplicate events also sent by the browser pixel
    /// for the same conversion. Optional, but recommended when both the pixel and the Conversions API
    /// report the same event.
    /// </summary>
    public string EventId { get; set; }
}
