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

        return collection + TableNameSeparator + type.Name;
    }

    public string GetDocumentTable(string collection = null)
    {
        if (String.IsNullOrEmpty(collection))
        {
            return DocumentTableName;
        }

        return collection + TableNameSeparator + DocumentTableName;
    }

    private string DocumentTableName
        => !String.IsNullOrWhiteSpace(_options.DocumentTable) ? _options.DocumentTable.Trim() : "Document";

    private string TableNameSeparator
        => (_options.TableNameSeparator ?? String.Empty).Trim();
}
