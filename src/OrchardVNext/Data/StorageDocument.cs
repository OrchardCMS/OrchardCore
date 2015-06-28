namespace OrchardVNext.Data {
    public abstract class StorageDocument {
        public int Id { get; set; }
        public abstract string Data { get; set; }
    }
}