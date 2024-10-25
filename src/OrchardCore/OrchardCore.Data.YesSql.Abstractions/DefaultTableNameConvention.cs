using YesSql;

namespace OrchardCore.Data;

public class DefaultTableNameConvention : ITableNameConvention
{
    private readonly DatabaseTableOptions _options;

    public DefaultTableNameConvention(DatabaseTableOptions options) => _options = options;

    public string GetIndexTable(Type type, string collection = null)
    {
        if (string.IsNullOrEmpty(collection))
        {
            return type.Name;
        }

        return $"{collection}{_options.TableNameSeparator}{type.Name}";
    }

    public string GetDocumentTable(string collection = null)
    {
        if (string.IsNullOrEmpty(collection))
        {
            return _options.DocumentTable;
        }

        return $"{collection}{_options.TableNameSeparator}{_options.DocumentTable}";
    }
}
