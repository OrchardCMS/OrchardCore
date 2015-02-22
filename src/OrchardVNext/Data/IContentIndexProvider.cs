using System;
using System.Collections.Generic;
using OrchardVNext.ContentManagement;

namespace OrchardVNext.Data {
    public interface IContentIndexProvider : IDependency {
        void Index(IContent content);
        IEnumerable<int> GetByFilter(Func<IContent, bool> filter);
    }
}