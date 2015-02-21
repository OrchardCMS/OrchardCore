using OrchardVNext.Data;

namespace OrchardVNext.ContentManagement.Records {
    [Persistent]
    public class ContentTypeRecord {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
    }
}