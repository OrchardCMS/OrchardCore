using System.Data;

namespace OrchardCore.Data;

public class DbExecutionContext : DbQueryContext
{
    public IsolationLevel TransactionIsolationLevel { get; set; } = IsolationLevel.Unspecified;

    public DbExecutionContext()
    {

    }

    public DbExecutionContext(IsolationLevel isolationLevel)
    {
        TransactionIsolationLevel = isolationLevel;
    }
}
