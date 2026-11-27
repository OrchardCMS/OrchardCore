using System.Linq.Expressions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Indexes;

namespace OrchardCore.Contents.Services;

// Runs the content filter pipeline against index records, without loading content documents.
internal sealed class ContentItemIndexQuery : IQuery<ContentItem>
{
    private readonly IQueryIndex<ContentItemIndex> _query;

    public ContentItemIndexQuery(IQueryIndex<ContentItemIndex> query)
    {
        _query = query;
    }

    public IQuery<ContentItem> Any(params Func<IQuery<ContentItem>, IQuery<ContentItem>>[] predicates)
    {
        _query.Any(predicates.Select(predicate => (Action<IQueryIndex<ContentItemIndex>>)(query =>
        {
            var projection = new ContentItemIndexQuery(query);
            projection.EnsureOwnership(predicate(projection));
        })).ToArray());
        return this;
    }

    public IQuery<ContentItem> All(params Func<IQuery<ContentItem>, IQuery<ContentItem>>[] predicates)
    {
        _query.All(predicates.Select(predicate => (Action<IQueryIndex<ContentItemIndex>>)(query =>
        {
            var projection = new ContentItemIndexQuery(query);
            projection.EnsureOwnership(predicate(projection));
        })).ToArray());
        return this;
    }

    public async ValueTask<IQuery<ContentItem>> AnyAsync(params Func<IQuery<ContentItem>, ValueTask<IQuery<ContentItem>>>[] predicates)
    {
        await _query.AnyAsync(predicates.Select(predicate => (Func<IQueryIndex<ContentItemIndex>, ValueTask>)(async query =>
        {
            var projection = new ContentItemIndexQuery(query);
            projection.EnsureOwnership(await predicate(projection));
        })).ToArray());
        return this;
    }

    public async ValueTask<IQuery<ContentItem>> AllAsync(params Func<IQuery<ContentItem>, ValueTask<IQuery<ContentItem>>>[] predicates)
    {
        await _query.AllAsync(predicates.Select(predicate => (Func<IQueryIndex<ContentItemIndex>, ValueTask>)(async query =>
        {
            var projection = new ContentItemIndexQuery(query);
            projection.EnsureOwnership(await predicate(projection));
        })).ToArray());
        return this;
    }

    public void EnsureOwnership(IQuery<ContentItem> query)
    {
        if (!ReferenceEquals(this, query) && !(query is IIndexQuery indexQuery && ReferenceEquals(this, indexQuery.Owner)))
        {
            throw new NotSupportedException("Content list filters must compose the supplied query to support grouping.");
        }
    }

    public IQuery<ContentItem, TIndex> With<TIndex>() where TIndex : class, IIndex
        => new IndexQuery<TIndex>(this, ((IQuery<ContentItemIndex>)_query).With<TIndex>());

    public IQuery<ContentItem, TIndex> With<TIndex>(Expression<Func<TIndex, bool>> predicate) where TIndex : class, IIndex
        => With<TIndex>().Where(predicate);

    public IQuery<ContentItem> With(Type indexType)
    {
        ((IQuery<ContentItemIndex>)_query).With(indexType);
        return this;
    }

    public string GetTypeAlias(Type type) => ((IQuery<ContentItemIndex>)_query).GetTypeAlias(type);
    public IQuery<ContentItem> Skip(int count) => throw new NotSupportedException("Apply paging after grouping the index records.");
    public IQuery<ContentItem> Take(int count) => throw new NotSupportedException("Apply paging after grouping the index records.");
    public IQuery<ContentItem> NoDuplicates() => this;
    public Task<int> CountAsync(CancellationToken cancellationToken = default) => _query.CountAsync(cancellationToken);
    public Task<ContentItem> FirstOrDefaultAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<ContentItem>> ListAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public IAsyncEnumerable<ContentItem> ToAsyncEnumerable(CancellationToken cancellationToken = default) => throw new NotSupportedException();

    private interface IIndexQuery
    {
        ContentItemIndexQuery Owner { get; }
    }

    private sealed class IndexQuery<TIndex> : IQuery<ContentItem, TIndex>, IIndexQuery where TIndex : class, IIndex
    {
        private readonly ContentItemIndexQuery _owner;
        private readonly IQuery<ContentItemIndex, TIndex> _index;

        public IndexQuery(ContentItemIndexQuery owner, IQuery<ContentItemIndex, TIndex> index)
        {
            _owner = owner;
            _index = index;
        }

        public ContentItemIndexQuery Owner => _owner;

        public IQuery<ContentItem, TIndex> Where(Expression<Func<TIndex, bool>> predicate)
        {
            _index.Where(predicate);
            return this;
        }

        public IQuery<ContentItem, TIndex> Where(string sql)
        {
            _index.Where(sql);
            return this;
        }

        public IQuery<ContentItem, TIndex> Where(Func<ISqlDialect, string> sql)
        {
            _index.Where(sql);
            return this;
        }

        public IQuery<ContentItem, TIndex> WithParameter(string name, object value)
        {
            _index.WithParameter(name, value);
            return this;
        }

        public IQuery<ContentItem, TIndex> OrderBy(Expression<Func<TIndex, object>> keySelector)
        {
            _index.OrderBy(keySelector);
            return this;
        }

        public IQuery<ContentItem, TIndex> OrderByDescending(Expression<Func<TIndex, object>> keySelector)
        {
            _index.OrderByDescending(keySelector);
            return this;
        }

        public IQuery<ContentItem, TIndex> ThenBy(Expression<Func<TIndex, object>> keySelector)
        {
            _index.ThenBy(keySelector);
            return this;
        }

        public IQuery<ContentItem, TIndex> ThenByDescending(Expression<Func<TIndex, object>> keySelector)
        {
            _index.ThenByDescending(keySelector);
            return this;
        }

        public IQuery<ContentItem, TIndex> OrderBy(string sql)
        {
            _index.OrderBy(sql);
            return this;
        }

        public IQuery<ContentItem, TIndex> OrderByDescending(string sql)
        {
            _index.OrderByDescending(sql);
            return this;
        }

        public IQuery<ContentItem, TIndex> ThenBy(string sql)
        {
            _index.ThenBy(sql);
            return this;
        }

        public IQuery<ContentItem, TIndex> ThenByDescending(string sql)
        {
            _index.ThenByDescending(sql);
            return this;
        }

        public IQuery<ContentItem, TIndex> OrderByRandom() => throw new NotSupportedException("Grouped paging requires deterministic sorting.");
        public IQuery<ContentItem, TIndex> ThenByRandom() => throw new NotSupportedException("Grouped paging requires deterministic sorting.");
        public IQuery<ContentItem, TOther> With<TOther>() where TOther : class, IIndex => _owner.With<TOther>();
        public IQuery<ContentItem, TOther> With<TOther>(Expression<Func<TOther, bool>> predicate) where TOther : class, IIndex => _owner.With(predicate);
        public IQuery<ContentItem> With(Type indexType) => _owner.With(indexType);
        public string GetTypeAlias(Type type) => _owner.GetTypeAlias(type);
        public IQuery<ContentItem> Any(params Func<IQuery<ContentItem>, IQuery<ContentItem>>[] predicates) => _owner.Any(predicates);
        public IQuery<ContentItem> All(params Func<IQuery<ContentItem>, IQuery<ContentItem>>[] predicates) => _owner.All(predicates);
        public ValueTask<IQuery<ContentItem>> AnyAsync(params Func<IQuery<ContentItem>, ValueTask<IQuery<ContentItem>>>[] predicates) => _owner.AnyAsync(predicates);
        public ValueTask<IQuery<ContentItem>> AllAsync(params Func<IQuery<ContentItem>, ValueTask<IQuery<ContentItem>>>[] predicates) => _owner.AllAsync(predicates);
        public IQuery<ContentItem> Skip(int count) => _owner.Skip(count);
        public IQuery<ContentItem> Take(int count) => _owner.Take(count);
        public IQuery<ContentItem> NoDuplicates() => this;
        public Task<int> CountAsync(CancellationToken cancellationToken = default) => _owner.CountAsync(cancellationToken);
        public Task<ContentItem> FirstOrDefaultAsync(CancellationToken cancellationToken = default) => _owner.FirstOrDefaultAsync(cancellationToken);
        public Task<IReadOnlyList<ContentItem>> ListAsync(CancellationToken cancellationToken = default) => _owner.ListAsync(cancellationToken);
        public IAsyncEnumerable<ContentItem> ToAsyncEnumerable(CancellationToken cancellationToken = default) => _owner.ToAsyncEnumerable(cancellationToken);
    }
}
