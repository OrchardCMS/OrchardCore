using System.Data.Common;

namespace OrchardCore.Data;

/// <summary>
/// A <see cref="DbConnection"/> that wraps the connection of a database provider, e.g. to trace its commands.
/// </summary>
public interface IDbConnectionWrapper
{
    /// <summary>
    /// Gets the wrapped connection.
    /// </summary>
    DbConnection InnerConnection { get; }
}

/// <summary>
/// Extensions of <see cref="DbConnection"/> related to <see cref="IDbConnectionWrapper"/>.
/// </summary>
public static class DbConnectionWrapperExtensions
{
    /// <summary>
    /// Returns the connection of the database provider, e.g. a 'SqliteConnection', that a connection may wrap.
    /// </summary>
    /// <param name="connection">The connection, wrapped or not.</param>
    public static DbConnection Unwrap(this DbConnection connection)
    {
        while (connection is IDbConnectionWrapper wrapper)
        {
            connection = wrapper.InnerConnection;
        }

        return connection;
    }
}
