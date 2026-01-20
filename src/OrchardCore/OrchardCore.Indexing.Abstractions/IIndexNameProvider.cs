namespace OrchardCore.Indexing;

public interface IIndexNameProvider
{
    string GetFullIndexName(string name);
}
