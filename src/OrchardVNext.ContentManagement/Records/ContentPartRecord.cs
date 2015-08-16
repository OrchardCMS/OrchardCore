namespace OrchardVNext.ContentManagement.Records {
    public abstract class ContentPartRecord {
        public virtual int Id { get; set; }
        public virtual ContentItemRecord ContentItemRecord { get; set; }
    }
}
