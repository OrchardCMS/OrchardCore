using OrchardCore.Abstractions.Indexing;
using OrchardCore.Indexing.Core.Indexes;
using OrchardCore.Indexing.Models;
using YesSql;

namespace OrchardCore.Indexing.Core;

public sealed class DefaultIndexProfileStore : IIndexProfileStore
{
    private readonly ISession _session;

    public DefaultIndexProfileStore(ISession session)
    {
        _session = session;
    }

    public ValueTask<bool> DeleteAsync(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        _session.Delete(indexProfile);

        return ValueTask.FromResult(true);
    }

    public async ValueTask<IndexProfile> FindByIdAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return await _session.Query<IndexProfile, IndexProfileIndex>()
            .Where(x => x.IndexProfileId == id)
            .FirstOrDefaultAsync();
    }

    public async ValueTask<IndexProfile> FindByNameAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return await _session.Query<IndexProfile, IndexProfileIndex>()
            .Where(x => x.Name == name)
            .FirstOrDefaultAsync();
    }

    public async ValueTask<IndexProfile> FindByIndexNameAndProviderAsync(string indexName, string providerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentException.ThrowIfNullOrEmpty(providerName);

        return await _session.Query<IndexProfile, IndexProfileIndex>()
            .Where(x => x.IndexName == indexName && x.ProviderName == providerName)
            .FirstOrDefaultAsync();
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetByTypeAsync(string type)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);

        return await _session.Query<IndexProfile, IndexProfileIndex>()
            .Where(x => x.Type == type)
            .ListAsync();
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetByProviderAsync(string providerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);

        return await _session.Query<IndexProfile, IndexProfileIndex>()
            .Where(x => x.ProviderName == providerName)
            .ListAsync();
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetAsync(string providerName, string type)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentException.ThrowIfNullOrEmpty(type);

        return await _session.Query<IndexProfile, IndexProfileIndex>()
             .Where(x => x.ProviderName == providerName && x.Type == type)
             .ListAsync();
    }

    public async ValueTask<PageResult<IndexProfile>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
        where TQuery : QueryContext
    {
        var records = BuildQuery(context);

        var skip = (page - 1) * pageSize;

        return new PageResult<IndexProfile>
        {
            Count = await records.CountAsync(),
            Models = await records.Skip(skip).Take(pageSize).ListAsync(),
        };
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetAllAsync()
    {
        return await _session.Query<IndexProfile, IndexProfileIndex>().ListAsync();
    }

    public ValueTask CreateAsync(IndexProfile indexProfile)
        => UpdateAsync(indexProfile);

    public async ValueTask UpdateAsync(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        if (string.IsNullOrEmpty(indexProfile.Id))
        {
            indexProfile.Id = IdGenerator.GenerateId();
        }

        var exists = await _session.QueryIndex<IndexProfileIndex>()
            .Where(x => x.IndexName == indexProfile.IndexName && x.ProviderName == indexProfile.ProviderName && x.IndexProfileId != indexProfile.Id)
            .FirstOrDefaultAsync();

        if (exists is not null)
        {
            throw new InvalidOperationException("There is already another index with the same name.");
        }

        var existsByName = await _session.QueryIndex<IndexProfileIndex>()
            .Where(x => x.Name == indexProfile.Name && x.IndexProfileId != indexProfile.Id)
            .FirstOrDefaultAsync();

        if (existsByName is not null)
        {
            throw new InvalidOperationException("There is already another index with the same name.");
        }

        await _session.SaveAsync(indexProfile, checkConcurrency: true);
    }

    public ValueTask SaveChangesAsync()
        => ValueTask.CompletedTask;

    private IQuery<IndexProfile, IndexProfileIndex> BuildQuery(QueryContext context)
    {
        var query = _session.Query<IndexProfile, IndexProfileIndex>();

        if (context == null)
        {
            return query;
        }

        if (!string.IsNullOrEmpty(context.Name))
        {
            query = query.Where(x => x.Name.Contains(context.Name));
        }

        if (context.Sorted)
        {
            query = query.OrderBy(x => x.Name);
        }

        return query;
    }
}
