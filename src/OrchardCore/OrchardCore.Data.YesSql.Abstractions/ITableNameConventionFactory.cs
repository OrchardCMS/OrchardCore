using YesSql;

namespace OrchardCore.Data.YesSql;

public interface ITableNameConventionFactory
{
    ITableNameConvention Create(DatabaseTableOptions options);
}
