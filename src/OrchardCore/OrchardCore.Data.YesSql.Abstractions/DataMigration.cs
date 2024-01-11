using YesSql.Sql;

namespace OrchardCore.Data.Migration
{
    /// <summary>
    /// A <see cref="DataMigration"/> is discovered through method reflection
    /// and runs sequentially from the Create() method through UpdateFromX().
    /// </summary>
    /// <example>
    /// Usage of method implementations:
    /// <code>
    /// public class Migrations : DataMigration
    /// {
    ///     public int Create() { return 1; } // or
    ///     public Task&lt;int&gt; CreateAsync() { return 1; }
    ///     public int UpdateFrom1() { return 2; } // or
    ///     public Task&lt;int&gt; UpdateFrom1Async() { return 2; }
    ///     public void Uninstall() { } // or
    ///     public Task UninstallAsync() { }
    /// }
    /// </code>
    /// </example>
    public abstract class DataMigration : IDataMigration
    {
        /// <inheritdocs />
        public ISchemaBuilder SchemaBuilder { get; set; }
    }
}
