using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Localization;

/// <summary>
/// Provides an extension methods for <see cref="IOrchardHelper"/>.
/// </summary>
#pragma warning disable CA1050 // Declare types in namespaces
public static class LocalizationOrchardHelperExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    public static Dictionary<string, string> GetJSLocalizations(this IOrchardHelper orchardHelper, params string[] groups)
    {
        var jsLocalizerServices = orchardHelper.HttpContext.RequestServices.GetServices<IJSLocalizer>();

        var result = new Dictionary<string, string>();

        foreach (var jsLocalizerService in jsLocalizerServices)
        {
            var jsLocDict = jsLocalizerService.GetLocalizations(groups);
            jsLocDict.Concat(result).ToList().ForEach(kvp => result[kvp.Key] = kvp.Value);
        }

        return result;
    }
}
