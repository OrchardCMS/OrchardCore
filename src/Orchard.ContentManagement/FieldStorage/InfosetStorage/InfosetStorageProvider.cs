using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.FieldStorage.InfosetStorage {
    public class InfosetStorageProvider : IFieldStorageProvider {
        public string ProviderName {
            get { return FieldStorageProviderSelector.DefaultProviderName; }
        }

        public IFieldStorage BindStorage(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition) {
            var partName = XmlConvert.EncodeLocalName(contentPart.PartDefinition.Name);
            var fieldName = XmlConvert.EncodeLocalName(partFieldDefinition.Name);
            var infosetPart = contentPart.As<InfosetPart>();

            return new SimpleFieldStorage(
                (name, valueType) => Get(infosetPart.ContentItem.VersionRecord == null ? infosetPart.Infoset.Element : infosetPart.VersionInfoset.Element, partName, fieldName, name),
                (name, valueType, value) => Set(infosetPart.ContentItem.VersionRecord == null ? infosetPart.Infoset.Element : infosetPart.VersionInfoset.Element, partName, fieldName, name, value));
        }

        private static string Get(XElement element, string partName, string fieldName, string valueName) {
            var partElement = element.Element(partName);
            if (partElement == null) {
                return null;
            }
            var fieldElement = partElement.Element(fieldName);
            if (fieldElement == null) {
                return null;
            }
            if (string.IsNullOrEmpty(valueName)) {
                return fieldElement.Value;
            }
            var valueAttribute = fieldElement.Attribute(XmlConvert.EncodeLocalName(valueName));
            if (valueAttribute == null) {
                return null;
            }
            return valueAttribute.Value;
        }

        private void Set(XElement element, string partName, string fieldName, string valueName, string value) {
            var partElement = element.Element(partName);
            if (partElement == null) {
                partElement = new XElement(partName);
                element.Add(partElement);
            }
            var fieldElement = partElement.Element(fieldName);
            if (fieldElement == null) {
                fieldElement = new XElement(fieldName);
                partElement.Add(fieldElement);
            }
            if (string.IsNullOrEmpty(valueName)) {
                fieldElement.Value = value;
            }
            else {
                fieldElement.SetAttributeValue(XmlConvert.EncodeLocalName(valueName), value);
            }
        }
    }
}