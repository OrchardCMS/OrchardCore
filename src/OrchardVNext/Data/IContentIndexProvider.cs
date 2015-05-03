using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.Data {
    public interface IContentIndexProvider : IDependency {
        void Index<T>(T document) where T : DocumentRecord;
        void DeIndex<T>(T document) where T : DocumentRecord;

        IEnumerable<T> Query<T>() where T : DocumentRecord;
        IEnumerable<T> Query<T>(Expression<Func<T, bool>> filter) where T : DocumentRecord;
    }
}