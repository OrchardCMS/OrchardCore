using System;
using System.Globalization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture;
        private readonly CultureInfo _originalUICulture;

        private CultureScope(string culture, string uiCulture, bool useUserOverride)
        {
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
        /// <param name="useUserOverride">Whether to use a user-selected culture settings or use the default system culture settings. Defaults to <c>true</c>.</param>
        public static CultureScope Create(string culture, bool useUserOverride = true)
            => Create(culture, culture, useUserOverride);

        /// <summary>
        /// Creates a scope with a given culture.
        /// </summary>
        /// <param name="culture">The culture that will be used within the scope.</param>
        /// <param name="uiCulture">The UI culture that will be used within the scope.</param>
        /// <param name="useUserOverride">Whether to use a user-selected culture settings or use the default system culture settings. Defaults to <c>true</c>.</param>
        public static CultureScope Create(string culture, string uiCulture, bool useUserOverride = true)
            => new(culture, uiCulture, useUserOverride);

        /// <summary>
        /// Creates a scope with a given culture.
        /// </summary>
        /// <param name="culture">The culture that will be used within the scope.</param>
        /// <param name="useUserOverride">Whether to use a user-selected culture settings or use the default system culture settings. Defaults to <c>true</c>.</param>
        public static CultureScope Create(CultureInfo culture, bool useUserOverride = true)
            => Create(culture, culture, useUserOverride);

        /// <summary>
        /// Creates a scope with a given culture.
        /// </summary>
        /// <param name="culture">The culture that will be used within the scope.</param>
        /// <param name="uiCulture">The UI culture that will be used within the scope.</param>
        /// <param name="useUserOverride">Whether to use a user-selected culture settings or use the default system culture settings. Defaults to <c>true</c>.</param>
        public static CultureScope Create(CultureInfo culture, CultureInfo uiCulture, bool useUserOverride = true)
            => new(culture.Name, uiCulture.Name, useUserOverride);

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
