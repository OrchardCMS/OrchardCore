using System.Collections.Generic;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.ContentManagement.Data {
    public interface IContentItemStore : IDependency {
        void Store(ContentItem contentItem);
        ContentItem Get(int id);
        ContentItem Get(int id, VersionOptions options);
        IReadOnlyList<ContentItem> GetMany(IEnumerable<int> ids);
    }
}