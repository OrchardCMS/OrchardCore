namespace OrchardCore.HealthChecks
{
    /// <summary>
    /// Represents a programmable health chekcs options.
    /// </summary>
    public class HealthChecksOptions
    {
        /// <summary>
        /// Gets or sets the health check URL. Default to "/health/live".
        /// </summary>
        public string Url { get; set; } = "/health/live";
    }
}
