using System.Globalization;
using OrchardCore;

public static class RazorHelperExtensions
{
    /// <summary>
    /// Returns the text writing directionality or the current request.
    /// </summary>
    /// <returns><c>"rtl"</c> if the current culture is Left To Right, empty otherwise.</returns>
    public static string CultureDir(this IOrchardHelper orchardHelper)
    {
        return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? "rtl" : "";
    }

    /// <summary>
    /// Returns the current culture name.
    /// </summary>
    public static string CultureName(this IOrchardHelper orchardHelper)
    {
        return CultureInfo.CurrentUICulture.Name;
    }
}
