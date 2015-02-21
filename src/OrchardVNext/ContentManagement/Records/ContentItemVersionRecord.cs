using OrchardVNext.ContentManagement.FieldStorage.InfosetStorage;
using OrchardVNext.Data;
using OrchardVNext.Data.Conventions;

namespace OrchardVNext.ContentManagement.Records {
    [Persistent]
    public class ContentItemVersionRecord {
        public ContentItemVersionRecord() {
            Infoset = new Infoset();
        }

        public int Id { get; set; }
        public ContentItemRecord ContentItemRecord { get; set; }
        public int Number { get; set; }

        public bool Published { get; set; }
        public bool Latest { get; set; }

        [StringLengthMax]
        public string Data { get { return Infoset.Data; } set { Infoset.Data = value; } }
        public Infoset Infoset { get; protected set; }
    }
}