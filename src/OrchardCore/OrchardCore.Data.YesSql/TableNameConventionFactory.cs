using OrchardCore.Data.YesSql;
using YesSql;

namespace OrchardCore.Data;

public class TableNameConventionFactory : ITableNameConventionFactory
{
    public ITableNameConvention Create(DatabaseTableInfo options)
    {
        return new DefaultTableNameConvention(options);
    }
}
