using System.Collections.Generic;

namespace OrchardCore.Localization
{
    public interface ILocalizationFileLocationProvider
    {
        IEnumerable<string> GetLocations(string cultureName);
    }
}
