using System.Xml.Linq;
using OrchardVNext.ContentManagement.MetaData.Models;

namespace OrchardVNext.ContentManagement.MetaData.Services {
    /// <summary>
    /// Abstraction to manage settings metadata on a content.
    /// </summary>
    public interface ISettingsFormatter : IDependency {
        /// <summary>
        /// Maps an XML element to a settings dictionary.
        /// </summary>
        /// <param name="element">The XML element to be mapped.</param>
        /// <returns>The settings dictionary.</returns>
        SettingsDictionary Map(XElement element);

        /// <summary>
        /// Maps a settings dictionary to an XML element.
        /// </summary>
        /// <param name="settingsDictionary">The settings dictionary.</param>
        /// <returns>The XML element.</returns>
        XElement Map(SettingsDictionary settingsDictionary);
    }
}
