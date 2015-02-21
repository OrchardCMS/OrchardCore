using System;
using System.Collections.Generic;
using OrchardVNext.ContentManagement;

namespace OrchardVNext.Demo.Services {
    public interface IContentIndexProvider : IDependency {
        void Index(ContentItem contentItem);
        IEnumerable<int> GetByFilter(Func<IContent, bool> filter);
    }
}