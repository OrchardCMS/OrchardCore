namespace OrchardCore.ContentManagement.GraphQL.Queries;

public interface IIndexPropertyProvider
{
    string IndexName { get; }
    bool TryGetValue(string propertyName, out string indexPropertyName);
}
