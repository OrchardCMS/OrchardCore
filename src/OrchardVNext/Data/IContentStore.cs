using System.Collections.Generic;
using OrchardVNext.ContentManagement;

namespace OrchardVNext.Data {
    public interface IContentStore : IDependency {
        void Store(ContentItem contentItem);
        ContentItem Get(int id);
        IEnumerable<ContentItem> GetMany(IEnumerable<int> ids);
    }
}