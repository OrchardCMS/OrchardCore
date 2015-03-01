using System;
using System.Collections.Generic;
using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.Data {
    public interface IContentDocumentStore : IDependency {
        void Store<T>(T document) where T : DocumentRecord;
        void Remove<T>(T document) where T : DocumentRecord;
        IEnumerable<T> Query<T>() where T : DocumentRecord;
        IEnumerable<T> Query<T>(Func<T, bool> filter) where T : DocumentRecord;
    }
}