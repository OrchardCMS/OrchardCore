using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Localization
{
    public interface ILocalizationFileLocationProvider
    {
        IEnumerable<IFileInfo> GetLocations(string cultureName);
    }
}
