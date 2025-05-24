using OrchardCore.Abstractions.Indexing;
using OrchardCore.Documents;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.Core;

public sealed class DefaultIndexEntityStore : IIndexEntityStore
{
    private readonly IDocumentManager<ModelDocument<IndexEntity>> _documentManager;

    public DefaultIndexEntityStore(IDocumentManager<ModelDocument<IndexEntity>> documentManager)
    {
        _documentManager = documentManager;
    }

    public async ValueTask<bool> DeleteAsync(IndexEntity model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var document = await _documentManager.GetOrCreateMutableAsync();

        if (!document.Records.TryGetValue(model.Id, out var existingInstance))
        {
            return false;
        }

        var removed = document.Records.Remove(model.Id);

        if (removed)
        {
            await _documentManager.UpdateAsync(document);
        }
        return removed;
    }

    public async ValueTask<IndexEntity> FindByIdAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var document = await _documentManager.GetOrCreateImmutableAsync();

        if (document.Records.TryGetValue(id, out var record))
        {
            return record;
        }

        return null;
    }

    public async ValueTask<IndexEntity> FindByNameAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await _documentManager.GetOrCreateImmutableAsync();

        return document.Records.Values.FirstOrDefault(x => string.Equals(x.IndexName, name, StringComparison.OrdinalIgnoreCase));
    }

    public async ValueTask<IEnumerable<IndexEntity>> GetAsync(string providerName)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);

        var document = await _documentManager.GetOrCreateImmutableAsync();

        return document.Records.Values.Where(x => x.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
    }

    public async ValueTask<IEnumerable<IndexEntity>> GetAsync(string providerName, string type)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentException.ThrowIfNullOrEmpty(type);

        var document = await _documentManager.GetOrCreateImmutableAsync();

        return document.Records.Values.Where(x => x.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase) && x.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
    }

    public async ValueTask<PageResult<IndexEntity>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
            where TQuery : QueryContext
    {
        var records = await LocateInstancesAsync(context);

        var skip = (page - 1) * pageSize;

        return new PageResult<IndexEntity>
        {
            Count = records.Count(),
            Models = records.Skip(skip).Take(pageSize).ToArray(),
        };
    }

    public async ValueTask<IEnumerable<IndexEntity>> GetAllAsync()
    {
        var document = await _documentManager.GetOrCreateImmutableAsync();
        return document.Records.Values;
    }

    public async ValueTask CreateAsync(IndexEntity record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var document = await _documentManager.GetOrCreateMutableAsync();

        if (document.Records.Values.Any(x => x.IndexName.Equals(record.IndexName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("There is already another index with the same name.");
        }

        if (string.IsNullOrEmpty(record.Id))
        {
            record.Id = IdGenerator.GenerateId();
        }

        document.Records[record.Id] = record;

        await _documentManager.UpdateAsync(document);
    }

    public async ValueTask UpdateAsync(IndexEntity record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var document = await _documentManager.GetOrCreateMutableAsync();

        if (document.Records.Values.Any(x => x.IndexName.Equals(record.IndexName, StringComparison.OrdinalIgnoreCase) && x.Id != record.Id))
        {
            throw new InvalidOperationException("There is already another index with the same name.");
        }

        if (string.IsNullOrEmpty(record.Id))
        {
            record.Id = IdGenerator.GenerateId();
        }

        document.Records[record.Id] = record;

        await _documentManager.UpdateAsync(document);
    }

    public ValueTask SaveChangesAsync()
    {
        return ValueTask.CompletedTask;
    }

    private async ValueTask<IEnumerable<IndexEntity>> LocateInstancesAsync(QueryContext context)
    {
        var document = await _documentManager.GetOrCreateImmutableAsync();

        if (context == null)
        {
            return document.Records.Values;
        }

        var records = document.Records.Values.AsEnumerable();

        records = GetSortable(context, records);
        return records;
    }

    private static IEnumerable<IndexEntity> GetSortable(QueryContext context, IEnumerable<IndexEntity> records)
    {
        if (!string.IsNullOrEmpty(context.Name))
        {
            records = records.Where(x => context.Name.Contains(x.DisplayText, StringComparison.OrdinalIgnoreCase));
        }

        if (context.Sorted)
        {
            records = records.OrderBy(x => x.DisplayText);
        }

        return records;
    }
}
