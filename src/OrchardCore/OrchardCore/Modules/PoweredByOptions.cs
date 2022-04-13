namespace OrchardCore.Modules
{
    /// <summary>
    /// Specifies options for the <see cref="PoweredByMiddleware"/>.
    /// </summary>
    public class PoweredByOptions
    {
        /// <summary>
        /// Gets or sets the X-PoweredBy HTTP header.
        /// </summary>
        public string Value { get; set; }
    }
}
