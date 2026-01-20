namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

internal sealed class DefaultDynamicIndexPropertyProvider : IIndexPropertyProvider
{
    public string IndexName => string.Empty;

    public bool TryGetValue(string propertyName, out string indexPropertyName)
    {
        var index = propertyName.LastIndexOf(':');

        if (index >= 0)
        {
            indexPropertyName = propertyName[(index + 1)..];
            return true;
        }

        indexPropertyName = null;
        return false;
    }
}
