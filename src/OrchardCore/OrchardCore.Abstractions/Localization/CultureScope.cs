using System;
using System.Globalization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a scope that you can change the current culture within.
    /// </summary>
    /// <remarks>
    /// The scope disallow the current culture depends on local computer settings by default.
    /// For more information refer to https://github.com/OrchardCMS/OrchardCore/issues/11228
    /// </remarks>
    public sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture;
        private readonly CultureInfo _originalUICulture;

        private CultureScope(string culture, string uiCulture, bool ignoreSystemSettings)
        {
            var useUserOverride = !ignoreSystemSettings;
            Culture = new CultureInfo(culture, useUserOverride);
            UICulture = new CultureInfo(uiCulture, useUserOverride);
            _originalCulture = CultureInfo.CurrentCulture;
            _originalUICulture = CultureInfo.CurrentUICulture;

            SetCultures(Culture, UICulture);
        }

        /// <summary>
        /// Gets the current culture.
        /// </summary>
        public CultureInfo Culture { get; }

        /// <summary>
        /// Get the current UI culture.
        /// </summary>
        public CultureInfo UICulture { get; }

        /// <summary>
        /// Creates a scope with a given culture.
        /// </summary>
        /// <param name="culture">The culture that will be used within the scope.</param>
        /// <param name="ignoreSystemSettings">Whether to ignore the system culture settings or not. Defaults to <c>false</c>.</param>
        public static CultureScope Create(string culture, bool ignoreSystemSettings = false)
            => Create(culture, culture, ignoreSystemSettings);

        /// <summary>
        /// Creates a scope with a given culture.
        /// </summary>
        /// <param name="culture">The culture that will be used within the scope.</param>
        /// <param name="uiCulture">The UI culture that will be used within the scope.</param>
        /// <param name="ignoreSystemSettings">Whether to ignore the system culture settings or not. Defaults to <c>false</c>.</param>
        public static CultureScope Create(string culture, string uiCulture, bool ignoreSystemSettings = false)
            => new(culture, uiCulture, ignoreSystemSettings);

        /// <summary>
        /// Creates a scope with a given culture.
        /// </summary>
        /// <param name="culture">The culture that will be used within the scope.</param>
        /// <param name="ignoreSystemSettings">Whether to ignore the system culture settings or not. Defaults to <c>false</c>.</param>
        public static CultureScope Create(CultureInfo culture, bool ignoreSystemSettings = false)
            => Create(culture, culture, ignoreSystemSettings);

        /// <summary>
        /// Creates a scope with a given culture.
        /// </summary>
        /// <param name="culture">The culture that will be used within the scope.</param>
        /// <param name="uiCulture">The UI culture that will be used within the scope.</param>
        /// <param name="ignoreSystemSettings">Whether to ignore the system culture settings or not. Defaults to <c>false</c>.</param>
        public static CultureScope Create(CultureInfo culture, CultureInfo uiCulture, bool ignoreSystemSettings = false)
            => new(culture.Name, uiCulture.Name, ignoreSystemSettings);

        /// <inheritdoc/>
        public void Dispose()
        {
            SetCultures(_originalCulture, _originalUICulture);
        }

        private static void SetCultures(CultureInfo culture, CultureInfo uiCulture)
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = uiCulture;
        }
    }
}
