using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Xunit.Sdk;

namespace OrchardCore.Testing;

/// <summary>
/// Represents an attribute to be used in test method to replace the
/// <see cref="Thread.CurrentThread" /> <see cref="CultureInfo.CurrentCulture" /> and
/// <see cref="CultureInfo.CurrentUICulture" /> with another culture(s).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class UseCultureAttribute : BeforeAfterTestAttribute
{
    private readonly Lazy<CultureInfo> _culture;
    private readonly Lazy<CultureInfo> _uiCulture;

    private CultureInfo _originalCulture;
    private CultureInfo _originalUICulture;

    /// <summary>
    /// Creates an instance of <see cref="UseCultureAttribute"/> with culture.
    /// </summary>
    /// <param name="culture">The name of the culture to replace the current thread culture with.</param>
    public UseCultureAttribute(string culture)
        : this(culture, culture)
    {
    }

    /// <summary>
    /// Creates an instance of <see cref="UseCultureAttribute"/> with culture and UI culture.
    /// </summary>
    /// <param name="culture">>The name of the culture to replace the current thread culture with.</param>
    /// <param name="uiCulture">>The name of the UI culture to replace the current thread UI culture with.</param>
    public UseCultureAttribute(string culture, string uiCulture)
        : this(new CultureInfo(culture), new CultureInfo(uiCulture))
    {
    }

    /// <summary>
    /// Creates an instance of <see cref="UseCultureAttribute"/> with culture and UI culture.
    /// </summary>
    /// <param name="culture">>The culture to replace the current thread culture with.</param>
    /// <param name="uiCulture">>The UI culture to replace the current thread UI culture with.</param>
    public UseCultureAttribute(CultureInfo culture, CultureInfo uiCulture)
    {
        _culture = new Lazy<CultureInfo>(() => culture);
        _uiCulture = new Lazy<CultureInfo>(() => uiCulture);
    }

    /// <summary>
    /// Gets the culture.
    /// </summary>
    public CultureInfo Culture => _culture.Value;

    /// <summary>
    /// Gets the UI culture.
    /// </summary>
    public CultureInfo UICulture => _uiCulture.Value;

    /// <inheritdoc/>
    public override void Before(MethodInfo methodUnderTest)
    {
        _originalCulture = Thread.CurrentThread.CurrentCulture;
        _originalUICulture = Thread.CurrentThread.CurrentUICulture;

        SetCultures(Culture, UICulture);
    }

    /// <inheritdoc/>
    public override void After(MethodInfo methodUnderTest) => SetCultures(_originalCulture, _originalUICulture);

    private static void SetCultures(CultureInfo culture, CultureInfo uiCulture)
    {
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = uiCulture;

        CultureInfo.CurrentCulture.ClearCachedData();
        CultureInfo.CurrentUICulture.ClearCachedData();
    }
}
