using OrchardVNext.ContentManagement;

namespace OrchardVNext.Demo.Models {
    public class TestContentPartB : ContentPart {
        public int Line {
            get { return this.Retrieve(x => x.Line); }
            set { this.Store(x => x.Line, value); }
        }
    }
}