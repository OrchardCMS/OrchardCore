using System.Collections.Generic;
using Orchard.DependencyInjection;

namespace Orchard.ContentManagement.Data {
    public interface IContentItemStore : IDependency {
        void Store(ContentItem contentItem);
        ContentItem Get(int id);
        ContentItem Get(int id, VersionOptions options);
        IReadOnlyList<ContentItem> GetMany(IEnumerable<int> ids);
    }
}