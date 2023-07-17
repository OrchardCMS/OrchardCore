using System.Globalization;
using OrchardCore;
using OrchardCore.Localization;

#pragma warning disable CA1050 // Declare types in namespaces
public static class RazorHelperExtensions
{
    /// <summary>
    /// Returns the text writing directionality or the current request.
    /// </summary>
    /// <returns><c>"rtl"</c> if the current culture is Left To Right, <c>"ltr"</c> otherwise.</returns>
    public static string CultureDir(this IOrchardHelper _)
    {
        return CultureInfo.CurrentUICulture.GetLanguageDirection();
    }

    /// <summary>
    /// Returns the current culture name.
    /// </summary>
    public static string CultureName(this IOrchardHelper _)
    {
        return CultureInfo.CurrentUICulture.Name;
    }
}
#pragma warning restore CA1050 // Declare types in namespaces
