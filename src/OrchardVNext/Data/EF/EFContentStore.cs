using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Data.Entity;

namespace OrchardVNext.Data.EF {
    public class EFContentStore : IContentStore {
        private readonly DataContext _dataContext;

        public EFContentStore(DataContext dataContext) {
            _dataContext = dataContext;
        }

        public async Task<TDocument> GetAsync<TDocument>(int id) where TDocument : StorageDocument {
            return await _dataContext.Set<TDocument>().SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<TDocument>> GetManyAsync<TDocument>(IEnumerable<int> ids) where TDocument : StorageDocument {
            return await Task.FromResult<IReadOnlyList<TDocument>>(_dataContext.Set<TDocument>()
                .Where(x => x.GetType().Name == typeof(TDocument).Name)
                .Where(x => ids.Contains(x.Id))
                .Cast<TDocument>()
                .ToList());
        }

        public async Task<IReadOnlyList<TDocument>> Query<TDocument>(Expression<Func<TDocument, bool>> map) where TDocument : StorageDocument {
            return await Task.FromResult<IReadOnlyList<TDocument>>(_dataContext.Set<TDocument>()
                .Where(x => x.GetType().Name == typeof(TDocument).Name)
                .Cast<TDocument>()
                .Where(map.Compile())
                .ToList());
        }

        public async Task RemoveAsync<TDocument>(int id) where TDocument : StorageDocument {
            await Task.Run(() => {
                _dataContext.Remove(_dataContext.Set<TDocument>().SingleOrDefault(d => d.Id == id));
            });
        }

        public async Task<int> StoreAsync<TDocument>(TDocument document) where TDocument : StorageDocument {
            return await Task.FromResult<int>(Task.Run(() => {
                _dataContext.Set<TDocument>().Add(document);
                return document.Id;
            }).Result);
        }
    }
}