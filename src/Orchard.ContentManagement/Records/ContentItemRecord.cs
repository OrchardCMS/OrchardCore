using System.Collections.Generic;

namespace Orchard.ContentManagement.Records {
    public class ContentItemRecord {
        public int Id { get; set; }
        public ContentItemRecord()
        {
            Parts = new List<ContentPart>();
        }
        
        public string ContentType { get; set; }
        public IList<ContentPart> Parts { get; set; }
    }

}