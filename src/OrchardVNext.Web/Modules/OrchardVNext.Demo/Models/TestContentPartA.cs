using System;
using OrchardVNext.ContentManagement;

namespace OrchardVNext.Demo.Models {
    public class TestContentPartA : ContentPart {
        public int Line {
            get { return this.Retrieve(x => x.Line); }
            set { this.Store(x => x.Line, value); }
        }
    }
}