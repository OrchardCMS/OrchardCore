using System;
using System.Collections.Generic;
using System.Linq;
using OrchardVNext.ContentManagement;

namespace OrchardVNext.Demo.Services {
    public class EFContentIndexProvider : IContentIndexProvider {
        public void Index(ContentItem contentItem) {

        }

        public IEnumerable<int> GetByFilter(Func<IContent, bool> filter) {
            return Enumerable.Empty<int>();
        }
    }
}