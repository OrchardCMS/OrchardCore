namespace OrchardCore.Modules;

/// <summary>
/// Provides local values of the current time and time zone.
/// </summary>
public interface ILocalClock
{
    [Obsolete("This property has been deprecated and will be removed in a future version. Please use GetLocalNowAsync() instead.")]
    Task<DateTimeOffset> LocalNowAsync { get; }

    /// <summary>
    /// Gets the time for the local time zone.
    /// </summary>

#pragma warning disable CS0618 // Type or member is obsolete
    Task<DateTimeOffset> GetLocalNowAsync()
        => LocalNowAsync;
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Returns the local time zone.
    /// </summary>
    Task<ITimeZone> GetLocalTimeZoneAsync();

    /// <summary>
    /// Converts a <see cref="DateTimeOffset" /> to the specified <see cref="ITimeZone" /> instance.
    /// </summary>
    Task<DateTimeOffset> ConvertToLocalAsync(DateTimeOffset dateTimeOffset);

    /// <summary>
    /// Converts a <see cref="DateTime" /> representing a local time to the UTC value.
    /// </summary>
    Task<DateTime> ConvertToUtcAsync(DateTime dateTime);
}
