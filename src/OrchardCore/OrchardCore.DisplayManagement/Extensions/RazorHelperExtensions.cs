using System.Globalization;
using Newtonsoft.Json.Linq;
using OrchardCore;
using OrchardCore.DisplayManagement;

public static class RazorHelperExtensions
{
    /// <summary>
    /// Returns the text writing directionality or the current request.
    /// </summary>
    /// <returns><c>"rtl"</c> if the current culture is Left To Right, <c>"ltr"</c> otherwise.</returns>
    public static string CultureDir(this IOrchardHelper orchardHelper)
    {
        return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? "rtl" : "ltr";
    }

    /// <summary>
    /// Returns the current culture name.
    /// </summary>
    public static string CultureName(this IOrchardHelper orchardHelper)
    {
        return CultureInfo.CurrentUICulture.Name;
    }

    /// <summary>
    /// Dump a shape to JSON
    /// </summary>
    /// <returns>JObject representation of the shape, at time of invocation.</returns>
    public static JObject ShapeDump(this IOrchardHelper orchardHelper, object shape)
    {
        // Will throw a InvalidCastException if object is not a shape.
        var iShape = (IShape)shape;
        return iShape.ShapeDump();
    }
}
