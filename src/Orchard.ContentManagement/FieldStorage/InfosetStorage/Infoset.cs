using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Orchard.ContentManagement.FieldStorage.InfosetStorage {
    public class Infoset {
        private XElement _element;

        private void SetElement(XElement value) {
            _element = value;
        }

        [Key]
        public int Id { get; set; }

        public XElement Element {
            get {
                return _element ?? (_element = new XElement("Data"));
            }
        }

        public string Data {
            get {
                return _element == null ? null : Element.ToString(SaveOptions.DisableFormatting);
            }
            set {
                SetElement(string.IsNullOrEmpty(value) ? null : XElement.Parse(value, LoadOptions.PreserveWhitespace));
            }
        }
    }
}