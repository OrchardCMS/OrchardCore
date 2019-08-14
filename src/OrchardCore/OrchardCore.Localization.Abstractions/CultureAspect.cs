using System.Globalization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a culture metadata for the content item.
    /// </summary>
    public class CultureAspect
    {
        /// <summary>
        /// Gets or sets the culture.
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets the language direction of the <see cref="Culture"/>.
        /// </summary>
        public string LanguageDirection
            => Culture.TextInfo.IsRightToLeft
                ? Localization.LanguageDirection.RTL
                : Localization.LanguageDirection.LTR;
    }
}