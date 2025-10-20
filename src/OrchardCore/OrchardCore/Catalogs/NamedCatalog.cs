using OrchardCore.Documents;

namespace OrchardCore.Catalogs;

public class NamedCatalog<T> : Catalog<T>, INamedCatalog<T>
    where T : CatalogItem, INameAwareModel
{
    public NamedCatalog(IDocumentManager<DictionaryDocument<T>> documentManager)
        : base(documentManager)
    {
    }

    public async ValueTask<T> FindByNameAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await DocumentManager.GetOrCreateImmutableAsync();

        var record = document.Records.Values.FirstOrDefault(x => OrdinalIgnoreCaseEquals(x.Name, name));

        return Clone(record);
    }

    protected override void Saving(T record, DictionaryDocument<T> document)
    {
        if (document.Records.Values.Any(x => OrdinalIgnoreCaseEquals(x.Name, record.Name) && x.ItemId != record.ItemId))
        {
            throw new InvalidOperationException("There is already another model with the same name.");
        }
    }

    protected static bool OrdinalIgnoreCaseEquals(string str1, string str2)
    {
        return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
    }
}
