using System.Collections.Generic;
using OrchardVNext.ContentManagement;

namespace OrchardVNext.Data {
    public interface IContentStore : IDependency {
        void Store(ContentItem contentItem);
        ContentItem Get(int id);
        ContentItem Get(int id, VersionOptions options);
        IEnumerable<ContentItem> GetMany(IEnumerable<int> ids);
    }
}