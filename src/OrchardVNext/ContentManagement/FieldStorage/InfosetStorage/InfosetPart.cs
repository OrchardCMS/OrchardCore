using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.ContentManagement.FieldStorage.InfosetStorage {
    public class InfosetPart : ContentPart {
        public InfosetPart() {
            Infoset = new Infoset();
            VersionInfoset = new Infoset();
        }

        public Infoset Infoset { get; set; }
        public Infoset VersionInfoset { get; set; }


        public string Get<TPart>(string fieldName) {
            return Get<TPart>(fieldName, null);
        }

        public string Get<TPart>(string fieldName, string valueName) {
            return Get(typeof(TPart).Name, fieldName, valueName, typeof(TPart).GetTypeInfo().IsAssignableFrom(typeof(ContentItemVersionRecord).GetTypeInfo()));
        }

        public string Get(string partName, string fieldName) {
            return Get(partName, fieldName, null, false);
        }

        public string GetVersioned(string partName, string fieldName) {
            return Get(partName, fieldName, null, true);
        }

        public string Get(string partName, string fieldName, string valueName, bool versionable = false) {

            var element = versionable ? VersionInfoset.Element : Infoset.Element;

            var partElement = element.Element(XmlConvert.EncodeName(partName));
            if (partElement == null) {
                return null;
            }
            var fieldElement = partElement.Element(XmlConvert.EncodeName(fieldName));
            if (fieldElement == null) {
                return null;
            }
            if (string.IsNullOrEmpty(valueName)) {
                return fieldElement.Value;
            }
            var valueAttribute = fieldElement.Attribute(XmlConvert.EncodeName(valueName));
            if (valueAttribute == null) {
                return null;
            }
            return valueAttribute.Value;
        }

        public void Set<TPart>(string fieldName, string valueName, string value) {
            Set<TPart>(fieldName, value);
        }

        public void Set<TPart>(string fieldName, string value) {
            Set(typeof(TPart).Name, fieldName, null, value, typeof(TPart).GetTypeInfo().IsAssignableFrom(typeof(ContentItemVersionRecord).GetTypeInfo()));
        }

        public void Set(string partName, string fieldName, string value) {
            Set(partName, fieldName, null, value, false);
        }

        public void SetVersioned(string partName, string fieldName, string value) {
            Set(partName, fieldName, null, value, true);
        }

        public void Set(string partName, string fieldName, string valueName, string value, bool versionable = false) {

            var element = versionable ? VersionInfoset.Element : Infoset.Element;

            var encodedPartName = XmlConvert.EncodeName(partName);
            var partElement = element.Element(encodedPartName);
            if (partElement == null) {
                partElement = new XElement(encodedPartName);
                Infoset.Element.Add(partElement);
            }

            var encodedFieldName = XmlConvert.EncodeName(fieldName);
            var fieldElement = partElement.Element(encodedFieldName);
            if (fieldElement == null) {
                fieldElement = new XElement(encodedFieldName);
                partElement.Add(fieldElement);
            }

            if (string.IsNullOrEmpty(valueName)) {
                fieldElement.Value = value ?? "";
            }
            else {
                fieldElement.SetAttributeValue(XmlConvert.EncodeName(valueName), value);
            }
        }
    }
}