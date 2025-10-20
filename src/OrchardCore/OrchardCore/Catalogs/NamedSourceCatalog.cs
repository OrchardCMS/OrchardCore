using OrchardCore.Documents;

namespace OrchardCore.Catalogs;

public class NamedSourceCatalog<T> : SourceCatalog<T>, INamedSourceCatalog<T>, ISourceCatalog<T>
    where T : CatalogItem, INameAwareModel, ISourceAwareModel
{
    public NamedSourceCatalog(IDocumentManager<DictionaryDocument<T>> documentManager)
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

    public async ValueTask<T> GetAsync(string name, string source)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(source);

        var document = await DocumentManager.GetOrCreateImmutableAsync();

        var record = document.Records.Values.FirstOrDefault(x => OrdinalIgnoreCaseEquals(x.Name, name) && OrdinalIgnoreCaseEquals(x.Source, source));

        return Clone(record);
    }

    protected static bool OrdinalIgnoreCaseEquals(string str1, string str2)
    {
        return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
    }

    protected override void Saving(T record, DictionaryDocument<T> document)
    {
        if (document.Records.Values.Any(x => OrdinalIgnoreCaseEquals(x.Name, record.Name) && x.ItemId != record.ItemId))
        {
            throw new InvalidOperationException("There is already another model with the same name.");
        }
    }
}
