using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Validation;

namespace Orchard.ContentManagement.MetaData.Services {
    /// <summary>
    /// The content definition reader is used to import both content type and content part definitions from a XML format.
    /// </summary>
    public class ContentDefinitionReader : IContentDefinitionReader {
        /// <summary>
        /// The settings formatter to be used to convert the settings between a XML format and a dictionary.
        /// </summary>
        private readonly ISettingsFormatter _settingsFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentDefinitionReader"/> class.
        /// </summary>
        /// <param name="settingsFormatter">The settings formatter to be used to convert the settings between a dictionary and an XML format.</param>
        public ContentDefinitionReader(ISettingsFormatter settingsFormatter) {
            Argument.ThrowIfNull(settingsFormatter, "settingsFormatter");

            _settingsFormatter = settingsFormatter;
        }

        /// <summary>
        /// Merges a given content type definition provided in a XML format into a content type definition builder.
        /// </summary>
        /// <param name="element">The XML content type definition.</param>
        /// <param name="contentTypeDefinitionBuilder">The content type definition builder.</param>
        public void Merge(XElement element, ContentTypeDefinitionBuilder contentTypeDefinitionBuilder) {
            Argument.ThrowIfNull(element, "element");
            Argument.ThrowIfNull(contentTypeDefinitionBuilder, "contentTypeDefinitionBuilder");

            // Merge name
            contentTypeDefinitionBuilder.Named(XmlConvert.DecodeName(element.Name.LocalName));


            // Merge display name
            if (element.Attribute("DisplayName") != null) {
                contentTypeDefinitionBuilder.DisplayedAs(element.Attribute("DisplayName").Value);
            }

            // Merge settings
            foreach (var setting in _settingsFormatter.Map(element)) {
                contentTypeDefinitionBuilder.WithSetting(setting.Key, setting.Value);
            }

            // Merge parts
            foreach (var iter in element.Elements()) {
                var partElement = iter;
                var partName = XmlConvert.DecodeName(partElement.Name.LocalName);
                if (partName == "remove") {
                    var nameAttribute = partElement.Attribute("name");
                    if (nameAttribute != null) {
                        contentTypeDefinitionBuilder.RemovePart(nameAttribute.Value);
                    }
                }
                else {
                    contentTypeDefinitionBuilder.WithPart(
                        partName,
                        partBuilder => {
                            foreach (var setting in _settingsFormatter.Map(partElement)) {
                                partBuilder.WithSetting(setting.Key, setting.Value);
                            }
                        });
                }
            }
        }

        /// <summary>
        /// Merges a given content part definition provided in a XML format into a content part definition builder.
        /// </summary>
        /// <param name="element">The XML content type definition.</param>
        /// <param name="contentPartDefinitionBuilder">The content part definition builder.</param>
        public void Merge(XElement element, ContentPartDefinitionBuilder contentPartDefinitionBuilder) {
            Argument.ThrowIfNull(element, "element");
            Argument.ThrowIfNull(contentPartDefinitionBuilder, "contentPartDefinitionBuilder");

            // Merge name
            contentPartDefinitionBuilder.Named(XmlConvert.DecodeName(element.Name.LocalName));

            // Merge settings
            foreach (var setting in _settingsFormatter.Map(element)) {
                contentPartDefinitionBuilder.WithSetting(setting.Key, setting.Value);
            }

            // Merge fields
            foreach (var iter in element.Elements()) {
                var fieldElement = iter;
                var fieldParameters = XmlConvert.DecodeName(fieldElement.Name.LocalName).Split(new[] { '.' }, 2);
                var fieldName = fieldParameters.FirstOrDefault();
                var fieldType = fieldParameters.Skip(1).FirstOrDefault();
                if (fieldName == "remove") {
                    var nameAttribute = fieldElement.Attribute("name");
                    if (nameAttribute != null) {
                        contentPartDefinitionBuilder.RemoveField(nameAttribute.Value);
                    }
                }
                else {
                    contentPartDefinitionBuilder.WithField(
                        fieldName,
                        fieldBuilder => {
                            fieldBuilder.OfType(fieldType);
                            foreach (var setting in _settingsFormatter.Map(fieldElement)) {
                                fieldBuilder.WithSetting(setting.Key, setting.Value);
                            }
                        });
                }
            }
        }
    }
}