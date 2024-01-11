using System.Globalization;

namespace OrchardCore.ContentLocalization
{
    /// <summary>
    /// Represents a culture metadata for the content item.
    /// </summary>
    public class CultureAspect
    {
        /// <summary>
        /// Gets or sets the culture.
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;

        public bool HasCulture { get; set; }
    }
}
