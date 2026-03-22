namespace OrchardCore.Media.Events;

/// <summary>
/// Data passed to the <see cref="IMediaEventHandler.MediaPermittedStorageAsync"/> event.
/// </summary>
public class MediaPermittedStorageContext
{
    /// <summary>
    /// Gets or sets the amount of the usable free space in bytes. This can represent available disk space, or some
    /// other limitation.
    /// </summary>
    public long? PermittedStorage { get; set; }

    /// <summary>
    /// Updates the value of <see cref="PermittedStorage"/>, but only if the <paramref name="newValue"/> is smaller.
    /// Use this to avoid accidentally breaking limitations set by other event handlers.
    /// </summary>
    public void Constrain(long newValue)
    {
        if (PermittedStorage is not { } currentValue || newValue < currentValue)
        {
            PermittedStorage = newValue;
        }
    }
}
