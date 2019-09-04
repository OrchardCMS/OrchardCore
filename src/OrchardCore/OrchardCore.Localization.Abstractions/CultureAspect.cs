using System.Globalization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a culture metadata for the content item.
    /// </summary>
    public class CultureAspect
    {
        private static readonly CultureInfo _defaultCulture = CultureInfo.InstalledUICulture ?? CultureInfo.InvariantCulture;

        /// <summary>
        /// Gets or sets the culture.
        /// </summary>
        public CultureInfo Culture { get; set; } = _defaultCulture;
    }
}