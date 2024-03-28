using System.Collections.Generic;

namespace OrchardCore.Localization;
public interface IJSLocalizer {
    /// <summary>
    /// This dictionary needs to be affected either here or
    /// in a .cshtml template for the po Extractor to find the strings to translate.
    /// </summary>
    /// <returns>Returns a list of localized strings or null</returns>
    public Dictionary<string, string> GetLocalizations(string[] groups);
}
