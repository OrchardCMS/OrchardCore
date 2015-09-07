using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Orchard.DependencyInjection;

namespace Orchard.Data {
    public interface IContentStore : IDependency {
        Task<TDocument> GetAsync<TDocument>(int id) where TDocument : StorageDocument;
        Task StoreAsync<TDocument>(TDocument document) where TDocument : StorageDocument;
        Task<IReadOnlyList<TDocument>> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument;
        Task<IReadOnlyList<TDocument>> GetManyAsync<TDocument>(IEnumerable<int> ids) where TDocument : StorageDocument;
        Task RemoveAsync<TDocument>(int id) where TDocument : StorageDocument;
    }
}