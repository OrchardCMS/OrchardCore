using OrchardVNext.Data;

namespace OrchardVNext.ContentManagement.Records {
    [Persistent]
    public class ContentTypeRecord {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}