using System;
using System.Linq;
using System.Linq.Expressions;

namespace OrchardVNext.Data {
    public class Orderable<T> {
        private IQueryable<T> _queryable;

        public Orderable(IQueryable<T> enumerable) {
            _queryable = enumerable;
        }

        public IQueryable<T> Queryable {
            get { return _queryable; }
        }

        public Orderable<T> Asc<TKey>(Expression<Func<T, TKey>> keySelector) {
            _queryable = _queryable
                .OrderBy(keySelector);
            return this;
        }

        public Orderable<T> Asc<TKey1, TKey2>(Expression<Func<T, TKey1>> keySelector1,
                                              Expression<Func<T, TKey2>> keySelector2) {
            _queryable = _queryable
                .OrderBy(keySelector1)
                .ThenBy(keySelector2);
            return this;
        }

        public Orderable<T> Asc<TKey1, TKey2, TKey3>(Expression<Func<T, TKey1>> keySelector1,
                                                     Expression<Func<T, TKey2>> keySelector2,
                                                     Expression<Func<T, TKey3>> keySelector3) {
            _queryable = _queryable
                .OrderBy(keySelector1)
                .ThenBy(keySelector2)
                .ThenBy(keySelector3);
            return this;
        }

        public Orderable<T> Desc<TKey>(Expression<Func<T, TKey>> keySelector) {
            _queryable = _queryable
                .OrderByDescending(keySelector);
            return this;
        }

        public Orderable<T> Desc<TKey1, TKey2>(Expression<Func<T, TKey1>> keySelector1,
                                               Expression<Func<T, TKey2>> keySelector2) {
            _queryable = _queryable
                .OrderByDescending(keySelector1)
                .ThenByDescending(keySelector2);
            return this;
        }

        public Orderable<T> Desc<TKey1, TKey2, TKey3>(Expression<Func<T, TKey1>> keySelector1,
                                                      Expression<Func<T, TKey2>> keySelector2,
                                                      Expression<Func<T, TKey3>> keySelector3) {
            _queryable = _queryable
                .OrderByDescending(keySelector1)
                .ThenByDescending(keySelector2)
                .ThenByDescending(keySelector3);
            return this;
        }
    }
}