using OrchardCore.Abstractions.Indexing;
using OrchardCore.Documents;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.Core;

public sealed class DefaultIndexProfileStore : IIndexProfileStore
{
    private readonly IDocumentManager<DictionaryDocument<IndexProfile>> _documentManager;

    public DefaultIndexProfileStore(IDocumentManager<DictionaryDocument<IndexProfile>> documentManager)
    {
        _documentManager = documentManager;
    }

    public async ValueTask<bool> DeleteAsync(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        var document = await _documentManager.GetOrCreateMutableAsync().ConfigureAwait(false);

        if (!document.Records.TryGetValue(indexProfile.Id, out var existingInstance))
        {
            return false;
        }

        var removed = document.Records.Remove(indexProfile.Id);

        if (removed)
        {
            await _documentManager.UpdateAsync(document).ConfigureAwait(false);
        }
        return removed;
    }

    public async ValueTask<IndexProfile> FindByIdAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var document = await _documentManager.GetOrCreateImmutableAsync().ConfigureAwait(false);

        if (document.Records.TryGetValue(id, out var record))
        {
            return record;
        }

        return null;
    }

    public async ValueTask<IndexProfile> FindByNameAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await _documentManager.GetOrCreateImmutableAsync().ConfigureAwait(false);

        return document.Records.Values.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async ValueTask<IndexProfile> FindByIndexNameAndProviderAsync(string indexName, string providerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentException.ThrowIfNullOrEmpty(providerName);

        var document = await _documentManager.GetOrCreateImmutableAsync().ConfigureAwait(false);

        return document.Records.Values.FirstOrDefault(x => string.Equals(x.IndexName, indexName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.ProviderName, providerName, StringComparison.OrdinalIgnoreCase));
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetByTypeAsync(string type)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);

        var document = await _documentManager.GetOrCreateImmutableAsync().ConfigureAwait(false);

        return document.Records.Values.Where(x => x.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetByProviderAsync(string providerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);

        var document = await _documentManager.GetOrCreateImmutableAsync().ConfigureAwait(false);

        return document.Records.Values.Where(x => x.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetAsync(string providerName, string type)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentException.ThrowIfNullOrEmpty(type);

        var document = await _documentManager.GetOrCreateImmutableAsync().ConfigureAwait(false);

        return document.Records.Values.Where(x => x.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase) && x.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
    }

    public async ValueTask<PageResult<IndexProfile>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
        where TQuery : QueryContext
    {
        var records = await LocateInstancesAsync(context).ConfigureAwait(false);

        var skip = (page - 1) * pageSize;

        return new PageResult<IndexProfile>
        {
            Count = records.Count(),
            Models = records.Skip(skip).Take(pageSize).ToArray(),
        };
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetAllAsync()
    {
        var document = await _documentManager.GetOrCreateImmutableAsync().ConfigureAwait(false);

        return document.Records.Values;
    }

    public async ValueTask CreateAsync(IndexProfile record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var document = await _documentManager.GetOrCreateMutableAsync().ConfigureAwait(false);

        if (document.Records.Values.Any(x => x.IndexName.Equals(record.IndexName, StringComparison.OrdinalIgnoreCase) &&
            x.ProviderName.Equals(record.ProviderName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("There is already another index with the same name.");
        }

        if (document.Records.Values.Any(x => x.Name.Equals(record.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("There is already another index with the same name.");
        }

        if (string.IsNullOrEmpty(record.Id))
        {
            record.Id = IdGenerator.GenerateId();
        }

        document.Records[record.Id] = record;

        await _documentManager.UpdateAsync(document).ConfigureAwait(false);
    }

    public async ValueTask UpdateAsync(IndexProfile record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var document = await _documentManager.GetOrCreateMutableAsync().ConfigureAwait(false);

        if (document.Records.Values.Any(x => x.IndexName.Equals(record.IndexName, StringComparison.OrdinalIgnoreCase) &&
            x.ProviderName.Equals(record.ProviderName, StringComparison.OrdinalIgnoreCase) &&
            x.Id != record.Id))
        {
            throw new InvalidOperationException("There is already another index with the same name.");
        }

        if (document.Records.Values.Any(x => x.Name.Equals(record.Name, StringComparison.OrdinalIgnoreCase) &&
            x.Id != record.Id))
        {
            throw new InvalidOperationException("There is already another index with the same name.");
        }

        if (string.IsNullOrEmpty(record.Id))
        {
            record.Id = IdGenerator.GenerateId();
        }

        document.Records[record.Id] = record;

        await _documentManager.UpdateAsync(document).ConfigureAwait(false);
    }

    public ValueTask SaveChangesAsync()
        => ValueTask.CompletedTask;

    private async ValueTask<IEnumerable<IndexProfile>> LocateInstancesAsync(QueryContext context)
    {
        var document = await _documentManager.GetOrCreateImmutableAsync().ConfigureAwait(false);

        if (context == null)
        {
            return document.Records.Values;
        }

        var records = document.Records.Values.AsEnumerable();

        records = GetSortable(context, records);

        return records;
    }

    private static IEnumerable<IndexProfile> GetSortable(QueryContext context, IEnumerable<IndexProfile> records)
    {
        if (!string.IsNullOrEmpty(context.Name))
        {
            records = records.Where(x => context.Name.Contains(x.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (context.Sorted)
        {
            records = records.OrderBy(x => x.Name);
        }

        return records;
    }
}
