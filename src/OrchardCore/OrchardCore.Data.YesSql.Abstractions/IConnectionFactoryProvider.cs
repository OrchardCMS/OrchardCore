using OrchardCore.Environment.Shell;
using YesSql;

namespace OrchardCore.Data;

public interface IConnectionFactoryProvider
{
    public IConnectionFactory GetFactory(string providerName, string connectionString);

    public IConnectionFactory GetFactory(ShellSettings shellSettings);
}
