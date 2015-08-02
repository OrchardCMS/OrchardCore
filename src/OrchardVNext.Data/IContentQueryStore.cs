using System;
using System.Linq.Expressions;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.Data {
    public interface IContentQueryStore : IDependency {
        void Index<TDocument>(TDocument document, Type contentStore) where TDocument : StorageDocument;
        ContentIndexResult<TDocument> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument;
        void DeIndex<TDocument>(int id, Type contentStore);
    }
}