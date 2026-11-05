namespace OrchardCore.Telemetry;

/// <summary>
/// The options of the telemetry emitted by Orchard Core, set by the component collecting it, e.g. OpenTelemetry.
/// </summary>
public sealed class OrchardCoreTelemetryOptions
{
    /// <summary>
    /// Gets or sets the duration from which the rendering of a shape is traced with its own span.
    /// Defaults to 50 milliseconds. <see cref="TimeSpan.Zero"/> traces every shape.
    /// </summary>
    public TimeSpan SlowShapeThreshold { get; set; } = TimeSpan.FromMilliseconds(50);
}
