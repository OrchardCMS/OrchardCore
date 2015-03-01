using System;
using System.Collections.Generic;
using System.Linq;
using OrchardVNext.ContentManagement;
using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.Data.EF {
    public class EFContentIndexProvider : IContentIndexProvider {
        public void Index(DocumentRecord content)
        {
            // Get Lambda and store this content.
            var data = content.Infoset.Data;
        }

        public IEnumerable<int> Query<T>() where T : DocumentRecord
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> Query<T>(Func<T, bool> filter) where T : DocumentRecord
        {
            throw new NotImplementedException();
        }
    }
}