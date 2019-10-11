using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json.Linq;
using OrchardCore;
using OrchardCore.DisplayManagement;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Razor;

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
    public static async Task<IHtmlContent> ShapeDump(this IOrchardDisplayHelper orchardHelper, object shape)
    {
        // Will throw a InvalidCastException if object is not a shape.
        var iShape = (IShape)shape;

        // Hmm so make this create a dump shape and return it
        // the dump shape could load code mirror, in readonly, with js syntax, and probably would format ok.
        var jObject = iShape.ShapeDump();

        var factory = orchardHelper.HttpContext.RequestServices.GetRequiredService<IShapeFactory>();

        var codeShape = await factory.New.JsonCode(jObject: jObject);

        return await orchardHelper.DisplayHelper.ShapeExecuteAsync(codeShape);
    }
}
