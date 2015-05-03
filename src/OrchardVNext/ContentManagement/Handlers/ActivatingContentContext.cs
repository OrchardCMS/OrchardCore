using OrchardVNext.ContentManagement.MetaData.Models;

namespace OrchardVNext.ContentManagement.Handlers {
    public class ActivatingContentContext {
        public string ContentType { get; set; }
        public ContentTypeDefinition Definition { get; set; }
        public ContentItemBuilder Builder { get; set; }
    }
}
