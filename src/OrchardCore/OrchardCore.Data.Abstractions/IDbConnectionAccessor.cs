using System.Data.Common;

namespace OrchardCore.Data
{
    /// <summary>
    /// Represents a contract to access the <see cref="DbConnection"/>.
    /// </summary>
    public interface IDbConnectionAccessor
    {
        /// <summary>
        /// Creates a database connection.
        /// </summary>
        DbConnection CreateConnection();
    }
}
