using OrchardCore.Catalogs.Models;
using OrchardCore.Documents;

namespace OrchardCore.Catalogs;

public class Catalog<T> : ICatalog<T>
    where T : CatalogItem
{
    protected readonly IDocumentManager<DictionaryDocument<T>> DocumentManager;

    public Catalog(IDocumentManager<DictionaryDocument<T>> documentManager)
    {
        DocumentManager = documentManager;
    }

    public async ValueTask<bool> DeleteAsync(T entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var document = await DocumentManager.GetOrCreateMutableAsync();

        if (!document.Records.TryGetValue(entry.ItemId, out var existingInstance))
        {
            return false;
        }

        Deleting(entry, document);

        var removed = document.Records.Remove(entry.ItemId);

        if (removed)
        {
            await DocumentManager.UpdateAsync(document);
        }

        return removed;
    }

    public async ValueTask<T> FindByIdAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var document = await DocumentManager.GetOrCreateImmutableAsync();

        if (document.Records.TryGetValue(id, out var record))
        {
            return Clone(record);
        }

        return null;
    }

    public async ValueTask<IReadOnlyCollection<T>> GetAsync(IEnumerable<string> ids)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var document = await DocumentManager.GetOrCreateImmutableAsync();

        return ids.Where(document.Records.ContainsKey)
                .Select(id => Clone(document.Records[id]))
                .ToArray();
    }

    public async ValueTask<PageResult<T>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
            where TQuery : QueryContext
    {
        var records = await LocateInstancesAsync(context);

        var skip = (page - 1) * pageSize;

        return new PageResult<T>
        {
            Count = records.Count(),
            Entries = records.Skip(skip).Take(pageSize).Select(Clone).ToArray(),
        };
    }

    public async ValueTask<IReadOnlyCollection<T>> GetAllAsync()
    {
        var document = await DocumentManager.GetOrCreateImmutableAsync();

        return document.Records.Values.Select(Clone).ToArray();
    }

    public async ValueTask CreateAsync(T record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var document = await DocumentManager.GetOrCreateMutableAsync();

        if (string.IsNullOrEmpty(record.ItemId))
        {
            record.ItemId = IdGenerator.GenerateId();
        }

        Saving(record, document);

        document.Records[record.ItemId] = record;

        await DocumentManager.UpdateAsync(document);
    }

    public async ValueTask UpdateAsync(T record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var document = await DocumentManager.GetOrCreateMutableAsync();

        if (string.IsNullOrEmpty(record.ItemId))
        {
            record.ItemId = IdGenerator.GenerateId();
        }

        Saving(record, document);

        document.Records[record.ItemId] = record;

        await DocumentManager.UpdateAsync(document);
    }

    public ValueTask SaveChangesAsync()
    {
        return ValueTask.CompletedTask;
    }

    protected virtual void Deleting(T model, DictionaryDocument<T> document)
    {
    }

    protected virtual async ValueTask<IEnumerable<T>> LocateInstancesAsync(QueryContext context)
    {
        var document = await DocumentManager.GetOrCreateImmutableAsync();

        if (context == null)
        {
            return document.Records.Values;
        }

        var records = GetSortable(context, document.Records.Values.AsEnumerable());

        return records;
    }

    protected virtual IEnumerable<T> GetSortable(QueryContext context, IEnumerable<T> records)
    {
        if (!string.IsNullOrEmpty(context.Name))
        {
            records = records.Where(x => (x is INameAwareModel named && named.Name.Contains(context.Name, StringComparison.OrdinalIgnoreCase)) ||
             (x is IDisplayTextAwareModel displayModel && displayModel.DisplayText.Contains(context.Name, StringComparison.OrdinalIgnoreCase)) ||
             (x is not INameAwareModel && x is not IDisplayTextAwareModel));
        }

        if (context.Sorted)
        {
            records = records.OrderBy(x => x is IDisplayTextAwareModel displayModel ? displayModel.DisplayText : x is INameAwareModel named ? named.Name : string.Empty);
        }

        return records;
    }

    protected virtual void Saving(T record, DictionaryDocument<T> document)
    {
    }

    protected static T Clone(T record)
    {
        if (record is ICloneable<T> cloneableOfT)
        {
            return cloneableOfT.Clone();
        }

        if (record is ICloneable clonable)
        {
            return (T)clonable.Clone();
        }

        return record;
    }
}
