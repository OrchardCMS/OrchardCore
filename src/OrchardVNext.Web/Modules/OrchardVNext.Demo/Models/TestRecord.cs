using OrchardVNext.Data;

namespace OrchardVNext.Demo.Models {
    [Persistent]
    public class TestRecord {
        public int Id { get; set; }
        public string TestLine { get; set; }
    }
}