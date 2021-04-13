using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YesSql;
using YesSql.Indexes;

namespace OrchardCore.ContentManagement
{
    public class ContentQuery<TIndex> : IQuery<ContentItem, TIndex> where TIndex : class, IIndex
    {
        private IQuery<ContentItem, TIndex> _query;
        private readonly IContentManager _contentManager;

        public ContentQuery(IQuery<ContentItem, TIndex> query, IContentManager contentManager)
        {
            _query = query;
            _contentManager = contentManager;
        }

        public async Task<ContentItem> FirstOrDefaultAsync()
        {
            var contentItem = await _query.FirstOrDefaultAsync();
            if (contentItem != null)
            {
                contentItem = await _contentManager.LoadAsync(contentItem);
            }

            return contentItem;
        }

        public async Task<IEnumerable<ContentItem>> ListAsync()
        {
            return await _contentManager.LoadAsync(await _query.ListAsync());
        }

        public async IAsyncEnumerable<ContentItem> ToAsyncEnumerable()
        {
            await foreach (var contentItem in _query.ToAsyncEnumerable())
            {
                yield return await _contentManager.LoadAsync(contentItem);
            }
        }

        public IQuery<ContentItem> All(params Func<IQuery<ContentItem>, IQuery<ContentItem>>[] predicates)
        {
            return new ContentQuery(_query.All(predicates), _contentManager);
        }

        public IQuery<ContentItem> Any(params Func<IQuery<ContentItem>, IQuery<ContentItem>>[] predicates)
        {
            return new ContentQuery(_query.Any(predicates), _contentManager);
        }

        public Task<int> CountAsync() => _query.CountAsync();

        public string GetTypeAlias(Type t) => _query.GetTypeAlias(t);


        public IQuery<ContentItem, TIndex> OrderBy(Expression<Func<TIndex, object>> keySelector)
        {
            _query = _query.OrderBy(keySelector);
            return this;
        }

        public IQuery<ContentItem, TIndex> OrderBy(string sql)
        {
            _query = _query.OrderBy(sql);
            return this;
        }

        public IQuery<ContentItem, TIndex> OrderByDescending(Expression<Func<TIndex, object>> keySelector)
        {
            _query = _query.OrderByDescending(keySelector);
            return this;
        }

        public IQuery<ContentItem, TIndex> OrderByDescending(string sql)
        {
            _query = _query.OrderByDescending(sql);
            return this;
        }

        public IQuery<ContentItem, TIndex> OrderByRandom()
        {
            _query = _query.OrderByRandom();
            return this;
        }

        public IQuery<ContentItem> Skip(int count)
        {
            return new ContentQuery(_query.Skip(count), _contentManager);
        }

        public IQuery<ContentItem> Take(int count)
        {
            return new ContentQuery(_query.Take(count), _contentManager);
        }

        public IQuery<ContentItem, TIndex> ThenBy(Expression<Func<TIndex, object>> keySelector)
        {
            _query = _query.ThenBy(keySelector);
            return this;
        }

        public IQuery<ContentItem, TIndex> ThenBy(string sql)
        {
            _query = _query.ThenBy(sql);
            return this;
        }

        public IQuery<ContentItem, TIndex> ThenByDescending(Expression<Func<TIndex, object>> keySelector)
        {
            _query = _query.ThenByDescending(keySelector);
            return this;
        }

        public IQuery<ContentItem, TIndex> ThenByDescending(string sql)
        {
            _query = _query.ThenByDescending(sql);
            return this;
        }

        public IQuery<ContentItem, TIndex> ThenByRandom()
        {
            _query = _query.ThenByRandom();
            return this;
        }

        public IQuery<ContentItem, TIndex> Where(string sql)
        {
            _query = _query.Where(sql);
            return this;
        }

        public IQuery<ContentItem, TIndex> Where(Func<ISqlDialect, string> sql)
        {
            _query = _query.Where(sql);
            return this;
        }

        public IQuery<ContentItem, TIndex> Where(Expression<Func<TIndex, bool>> predicate)
        {
            _query = _query.Where(predicate);
            return this;
        }

        public IQuery<ContentItem> With(Type indexType)
        {
            return new ContentQuery(_query.With(indexType), _contentManager);
        }

        public IQuery<ContentItem, TIndex> WithParameter(string name, object value)
        {
            _query = _query.WithParameter(name, value);
            return this;
        }

        IQuery<ContentItem, TIndex1> IQuery<ContentItem>.With<TIndex1>()
        {
            return new ContentQuery<TIndex1>(_query.With<TIndex1>(), _contentManager);
        }

        IQuery<ContentItem, TIndex1> IQuery<ContentItem>.With<TIndex1>(Expression<Func<TIndex1, bool>> predicate)
        {
            return new ContentQuery<TIndex1>(_query.With(predicate), _contentManager);
        }
    }
}
