using System.Xml;
using System.Xml.Linq;
using OrchardVNext.ContentManagement.MetaData.Models;
using OrchardVNext.Validation;

namespace OrchardVNext.ContentManagement.MetaData.Services {
    /// <summary>
    /// The content definition writer is used to export both content type and content part definitions to a XML format.
    /// </summary>
    public class ContentDefinitionWriter : IContentDefinitionWriter {
        /// <summary>
        /// The settings formatter to be used to convert the settings between a dictionary and an XML format.
        /// </summary>
        private readonly ISettingsFormatter _settingsFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentDefinitionWriter"/> class.
        /// </summary>
        /// <param name="settingsFormatter">The settings formatter to be used to convert the settings between a dictionary and an XML format.</param>
        public ContentDefinitionWriter(ISettingsFormatter settingsFormatter) {
            Argument.ThrowIfNull(settingsFormatter, nameof(settingsFormatter));

            _settingsFormatter = settingsFormatter;
        }

        /// <summary>
        /// Exports a content type definition to a XML format.
        /// </summary>
        /// <param name="contentTypeDefinition">The type definition to be exported.</param>
        /// <returns>The content type definition in an XML format.</returns>
        public XElement Export(ContentTypeDefinition contentTypeDefinition) {
            Argument.ThrowIfNull(contentTypeDefinition, nameof(contentTypeDefinition));

            var typeElement = NewElement(contentTypeDefinition.Name, contentTypeDefinition.Settings);
            if (typeElement.Attribute("DisplayName") == null && contentTypeDefinition.DisplayName != null) {
                typeElement.Add(new XAttribute("DisplayName", contentTypeDefinition.DisplayName));
            }

            foreach (var typePart in contentTypeDefinition.Parts) {
                typeElement.Add(NewElement(typePart.PartDefinition.Name, typePart.Settings));
            }

            return typeElement;
        }

        /// <summary>
        /// Exports a content part definition to a XML format.
        /// </summary>
        /// <param name="contentPartDefinition">The part definition to be exported.</param>
        /// <returns>The content part definition in a XML format.</returns>
        public XElement Export(ContentPartDefinition contentPartDefinition) {
            Argument.ThrowIfNull(contentPartDefinition, nameof(contentPartDefinition));

            var partElement = NewElement(contentPartDefinition.Name, contentPartDefinition.Settings);
            foreach (var partField in contentPartDefinition.Fields) {
                var attributeName = string.Format("{0}.{1}", partField.Name, partField.FieldDefinition.Name);
                var partFieldElement = NewElement(attributeName, partField.Settings);
                partElement.Add(partFieldElement);
            }

            return partElement;
        }

        /// <summary>
        /// Builds a new XML element with a given name and a settings dictionary.
        /// </summary>
        /// <param name="name">The name of the element to be mapped to XML.</param>
        /// <param name="settings">The settings dictionary to be used as the element's attributes.</param>
        /// <returns>The new XML element.</returns>
        private XElement NewElement(string name, SettingsDictionary settings) {
            XElement element = _settingsFormatter.Map(settings);
            element.Name = XmlConvert.EncodeLocalName(name);

            return element;
        }
    }
}
