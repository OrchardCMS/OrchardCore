using System.Data;
using System.Threading;

namespace OrchardCore.Data;

public class DbExecutionContext
{
    public readonly static DbExecutionContext Instance = new();

    public CancellationToken CancellationToken { get; set; } = default;

    /// <summary>
    /// If set to true, any failure in the underlying commands will result in an exception.
    /// </summary>
    public bool ThrowException { get; set; } = true;

    public IsolationLevel TransactionIsolationLevel { get; set; } = IsolationLevel.Unspecified;

    public DbExecutionContext()
    {

    }

    public DbExecutionContext(IsolationLevel isolationLevel)
    {
        TransactionIsolationLevel = isolationLevel;
    }
}
