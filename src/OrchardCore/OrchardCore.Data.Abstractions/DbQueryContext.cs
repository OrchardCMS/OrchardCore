using System.Threading;

namespace OrchardCore.Data;

public class DbQueryContext
{
    public CancellationToken CancellationToken { get; set; } = default;

    /// <summary>
    /// If set to true, any failure in the underlying commands will result in an exception.
    /// </summary>
    public bool ThrowException { get; set; } = true;

    public DbQueryContext()
    {

    }

    public DbQueryContext(bool throwException, CancellationToken cancellationToken = default)
    {
        ThrowException = throwException;
        CancellationToken = cancellationToken;
    }
}
