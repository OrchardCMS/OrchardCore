using System.Collections.Generic;

namespace Orchard.Localization
{
    public interface ILocalizationFileLocationProvider
    {
        IEnumerable<string> GetLocations(string cultureName);
    }
}
