using System.ComponentModel.DataAnnotations;

namespace OrchardVNext.Data {
    public abstract class StorageDocument {
        [Key]
        public int Id { get; set; }
        public abstract string Data { get; set; }
    }
}