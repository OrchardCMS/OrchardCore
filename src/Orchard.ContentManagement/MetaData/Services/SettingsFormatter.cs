using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData.Services {
    /// <summary>
    /// Abstraction to manage settings metadata on a content.
    /// </summary>
    public class SettingsFormatter : ISettingsFormatter {
        /// <summary>
        /// Maps an XML element to a settings dictionary.
        /// </summary>
        /// <param name="element">The XML element to be mapped.</param>
        /// <returns>The settings dictionary.</returns>
        public SettingsDictionary Map(XElement element) {
            if (element == null) {
                return new SettingsDictionary();
            }

            return new SettingsDictionary(
                element.Attributes()
                    .ToDictionary(attr => XmlConvert.DecodeName(attr.Name.LocalName), attr => attr.Value));
        }

        /// <summary>
        /// Maps a settings dictionary to an XML element.
        /// </summary>
        /// <param name="settingsDictionary">The settings dictionary.</param>
        /// <returns>The XML element.</returns>
        public XElement Map(SettingsDictionary settingsDictionary) {
            if (settingsDictionary == null) {
                return new XElement("settings");
            }

            return new XElement(
                "settings", 
                settingsDictionary
                    .Where(kv => kv.Value != null)
                    .Select(kv => new XAttribute(XmlConvert.EncodeLocalName(kv.Key), kv.Value)));
        }
    }
}
