using System.Collections.Generic;
using OrchardVNext.ContentManagement.FieldStorage.InfosetStorage;
using OrchardVNext.Data;
using OrchardVNext.Data.Conventions;

namespace OrchardVNext.ContentManagement.Records {
    [Persistent]
    public class ContentItemRecord {
        public ContentItemRecord() {
            Versions = new List<ContentItemVersionRecord>();
            Infoset = new Infoset();
        }

        public int Id { get; set; }
        public ContentTypeRecord ContentType { get; set; }
        public IList<ContentItemVersionRecord> Versions { get; set; }

        [StringLengthMax]
        public string Data { get { return Infoset.Data; } set { Infoset.Data = value; } }
        public Infoset Infoset { get; protected set; }
    }
}