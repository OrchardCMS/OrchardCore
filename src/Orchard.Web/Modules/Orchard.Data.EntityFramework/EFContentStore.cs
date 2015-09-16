using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Data.Entity;

namespace Orchard.Data.EntityFramework {
    [OrchardFeature("Orchard.Data.EntityFramework")]
    public class EFContentStore : IContentStore {
        private readonly DataContext _dataContext;

        public EFContentStore(DataContext dataContext) {
            _dataContext = dataContext;
        }

        public async Task<TDocument> GetAsync<TDocument>(int id) where TDocument : StorageDocument {
            return await _dataContext.Set<TDocument>().SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<TDocument>> GetManyAsync<TDocument>(IEnumerable<int> ids) where TDocument : StorageDocument {
            return await _dataContext.Set<TDocument>()
                .Where(x => x.GetType().Name == typeof(TDocument).Name)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<TDocument>> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument {
            return await Task.FromResult(_dataContext.Set<TDocument>()
                .Where(x => x.GetType().Name == typeof(TDocument).Name)
                .Where(map.Compile())
                .ToList());
        }

        public async Task RemoveAsync<TDocument>(int id) where TDocument : StorageDocument {
            _dataContext.Remove(_dataContext.Set<TDocument>().SingleOrDefault(d => d.Id == id));

            await _dataContext.SaveChangesAsync();
        }

        public async Task StoreAsync<TDocument>(TDocument document) where TDocument : StorageDocument {
            _dataContext.Set<TDocument>().Add(document, GraphBehavior.SingleObject);
            
            await _dataContext.SaveChangesAsync();
        }
    }
}