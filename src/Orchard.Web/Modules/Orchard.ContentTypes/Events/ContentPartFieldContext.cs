using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentTypes.Events {
    public class ContentPartFieldContext {
        public string ContentPartName{ get; set; }
        public string ContentFieldName { get; set; }
    }
}