using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Contract that provides a location for the localization strings in the file system.
    /// </summary>
    public interface ILocalizationFileLocationProvider
    {
        /// <summary>
        /// Gets the localization location for a specified culture.
        /// </summary>
        /// <param name="cultureName">The culture name.</param>
        /// <returns>A list of localization files.</returns>
        IEnumerable<IFileInfo> GetLocations(string cultureName);
    }
}
