namespace Orchard.ContentManagement.Records {
    public abstract class ContentPartVersionRecord : ContentPartRecord {
        public virtual ContentItemVersionRecord ContentItemVersionRecord { get; set; }
    }
}