using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.Media.Services;


/// <summary>
/// TODO : Remove as it is only for testing merging dictionnaries
/// </summary>
public class NullJSLocalizer: IJSLocalizer
{
    private readonly IStringLocalizer S;

    public NullJSLocalizer(IStringLocalizer<NullJSLocalizer> localizer)
    {
        S = localizer;
    }

    /// <summary>
    /// This dictionary needs to be affected either here or
    /// in a .cshtml template for the po Extractor to find the strings to translate.
    /// </summary>
    /// <returns>Returns a list of localized strings</returns>
    public Dictionary<string, string> GetLocalizations(string[] groups)
    {
        var dictionary = new Dictionary<string, string>();

        if (groups.Contains("confirm-modal"))
        {
            dictionary.Add("Yes", S["Yes"].Value);
            dictionary.Add("No", S["No"].Value);
            dictionary.Add("Cancel", S["Cancel"].Value);
            dictionary.Add("Close", S["Close"].Value);
            dictionary.Add("Jasmin", S["Jasmin"].Value);
        }

        if (groups.Contains("image-upload"))
        {
            dictionary.Add("Crop", S["Crop"].Value);
            dictionary.Add("Cancel", S["Cancel"].Value);
        }

        return dictionary;
    }
}
