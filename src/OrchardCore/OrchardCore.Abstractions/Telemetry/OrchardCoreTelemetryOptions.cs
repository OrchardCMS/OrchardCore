namespace OrchardCore.Telemetry;

/// <summary>
/// The options of the telemetry emitted by Orchard Core, set by the component collecting it, e.g. OpenTelemetry.
/// </summary>
public sealed class OrchardCoreTelemetryOptions
{
    /// <summary>
    /// Gets or sets whether the spans of the database operations include their SQL text, in 'db.query.text'.
    /// The commands run by YesSql are parameterized, so their text holds no values. Defaults to <see langword="true"/>.
    /// </summary>
    public bool IncludeDatabaseQueryText { get; set; } = true;
}
