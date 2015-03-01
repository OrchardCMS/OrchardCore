using System;
using System.Collections.Generic;
using System.Linq;
using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.Data.EF
{
    public class EFContentDocumentStore : IContentDocumentStore {
        private readonly DataContext _dataContext;

        public EFContentDocumentStore(DataContext dataContext) {
            _dataContext = dataContext;
        }

        public void Store<T>(T document) where T : DocumentRecord {
            _dataContext.Add(document);
        }

        public void Remove<T>(T document) where T : DocumentRecord {
            _dataContext.Remove(document);
        }

        public IEnumerable<T> Query<T>() where T : DocumentRecord {
            return _dataContext.Set<T>().AsQueryable();
        }

        public IEnumerable<T> Query<T>(Func<T, bool> filter) where T : DocumentRecord {
            return _dataContext.Set<T>().Where(filter);
        }
    }
}