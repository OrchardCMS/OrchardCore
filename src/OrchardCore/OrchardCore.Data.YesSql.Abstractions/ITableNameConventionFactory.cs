using YesSql;

namespace OrchardCore.Data.YesSql;

public interface ITableNameConventionFactory
{
    ITableNameConvention Create(DatabaseTableInfo options);
}
