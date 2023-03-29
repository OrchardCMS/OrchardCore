using System;
using YesSql;

namespace OrchardCore.Data;

public class DefaultTableNameConvention : ITableNameConvention
{
    private readonly DatabaseTableOptions _options;

    public DefaultTableNameConvention(DatabaseTableOptions options) => _options = options;

    public string GetIndexTable(Type type, string collection = null)
    {
        if (String.IsNullOrEmpty(collection))
        {
            return type.Name;
        }

        return $"{collection}{_options.TableNameSeparator}{type.Name}";
    }

    public string GetDocumentTable(string collection = null)
    {
        if (String.IsNullOrEmpty(collection))
        {
            return _options.DocumentTable;
        }

        return $"{collection}{_options.TableNameSeparator}{_options.DocumentTable}";
    }
}
