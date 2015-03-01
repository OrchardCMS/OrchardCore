using System;
using System.Collections.Generic;
using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.Data {
    public interface IContentIndexProvider : IDependency {
        void Index(DocumentRecord content);

        IEnumerable<int> Query<T>() where T : DocumentRecord;

        IEnumerable<int> Query<T>(Func<T, bool> filter) where T : DocumentRecord;
    }
}