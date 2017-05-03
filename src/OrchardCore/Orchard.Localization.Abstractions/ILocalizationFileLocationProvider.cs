using System;
using System.Collections.Generic;
using System.Text;

namespace Orchard.Localization.Abstractions
{
    public interface ILocalizationFileLocationProvider
    {
        IEnumerable<string> GetLocations(string cultureName);
    }
}
