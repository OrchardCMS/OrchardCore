using System.Collections.Generic;

namespace OrchardCore.Localization
{
    public interface ILocalizationFileLocationProvider
    {
        int Order { get; }
        IEnumerable<string> GetLocations(string cultureName);
    }
}
