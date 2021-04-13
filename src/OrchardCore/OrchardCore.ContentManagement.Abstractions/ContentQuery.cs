using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.ContentManagement
{
    public class ContentQuery : IQuery<ContentItem>
    {
        private IQuery<ContentItem> _query;
        private readonly IContentManager _contentManager;

        public ContentQuery(IQuery<ContentItem> query, IContentManager contentManager)
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
            _query = _query.All(predicates);
            return this;
        }

        public IQuery<ContentItem> Any(params Func<IQuery<ContentItem>, IQuery<ContentItem>>[] predicates)
        {
            _query = _query.Any(predicates);
            return this;
        }

        public Task<int> CountAsync() => _query.CountAsync();

        public string GetTypeAlias(Type t) => _query.GetTypeAlias(t);

        public IQuery<ContentItem> Skip(int count)
        {
            _query = _query.Skip(count);
            return this;
        }

        public IQuery<ContentItem> Take(int count)
        {
            _query = _query.Take(count);
            return this;
        }

        public IQuery<ContentItem> With(Type indexType)
        {
            _query = _query.With(indexType);
            return this;
        }

        IQuery<ContentItem, TIndex> IQuery<ContentItem>.With<TIndex>()
        {
            return new ContentQuery<TIndex>(_query.With<TIndex>(), _contentManager);
        }

        IQuery<ContentItem, TIndex> IQuery<ContentItem>.With<TIndex>(Expression<Func<TIndex, bool>> predicate)
        {
            return new ContentQuery<TIndex>(_query.With(predicate), _contentManager);
        }
    }
}
