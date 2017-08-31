using System.Collections.Generic;

namespace Orchard.Localization.Abstractions
{
    public interface ILocalizationFileLocationProvider
    {
        IEnumerable<string> GetLocations(string cultureName);
    }
}
