using System;
using YesSql;

namespace OrchardCore.Data;

public class DefaultTableNameConvention : ITableNameConvention
{
    private readonly DatabaseTableOptions _options;

    public DefaultTableNameConvention(DatabaseTableOptions options)
    {
        _options = options;
    }

    public string GetIndexTable(Type type, string collection = null)
    {
        if (String.IsNullOrEmpty(collection))
        {
            return type.Name;
        }

        return collection + (_options.TablePrefixSeparator ?? String.Empty) + type.Name;
    }

    public string GetDocumentTable(string collection = null)
    {
        if (String.IsNullOrEmpty(collection))
        {
            return DocumentTable;
        }

        return collection + (_options.TablePrefixSeparator ?? String.Empty) + DocumentTable;
    }

    private string DocumentTable => !String.IsNullOrWhiteSpace(_options.DocumentTable) ? _options.DocumentTable : "Document";
}
