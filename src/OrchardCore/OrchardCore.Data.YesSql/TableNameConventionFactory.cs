using OrchardCore.Data.YesSql;
using YesSql;

namespace OrchardCore.Data;

public class TableNameConventionFactory : ITableNameConventionFactory
{
    public ITableNameConvention Create(DatabaseTableOptions options)
    {
        return new DefaultTableNameConvention(options);
    }
}
