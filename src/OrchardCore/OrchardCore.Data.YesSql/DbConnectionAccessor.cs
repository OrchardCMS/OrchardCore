using System.Data.Common;
using YesSql;

namespace OrchardCore.Data;

/// <summary>
/// Represents an accessor to the database connection.
/// </summary>
public class DbConnectionAccessor : IDbConnectionAccessor
{
    private readonly IStore _store;

    /// <summary>
    /// Creates a new instance of the <see cref="DbConnectionAccessor"/>.
    /// </summary>
    /// <param name="store">The <see cref="IStore"/>.</param>
    public DbConnectionAccessor(IStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        _store = store;
    }

    /// <inheritdocs />
    public DbConnection CreateConnection()
    {
        return _store.Configuration.ConnectionFactory.CreateConnection();
    }
}
