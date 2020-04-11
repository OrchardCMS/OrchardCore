using System.Globalization;

namespace OrchardCore.Localization.Models
{
    /// <summary>
    /// Represents a culture entry.
    /// </summary>
    public class CultureEntry
    {
        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> instance for the current culture entry.
        /// </summary>
        public CultureInfo CultureInfo { get; set; }

        /// <summary>
        /// Gets or sets whether the culture is the default one.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets whether the culture is supported.
        /// </summary>
        public bool Supported { get; set; }
    }
}
