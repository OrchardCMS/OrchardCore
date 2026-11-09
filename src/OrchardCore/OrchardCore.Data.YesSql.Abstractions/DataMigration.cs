using YesSql.Sql;

namespace OrchardCore.Data.Migration;

/// <summary>
/// A <see cref="DataMigration"/> is discovered through method reflection
/// and runs sequentially from the public static or instance Create() method through UpdateFromX().
/// </summary>
/// <example>
/// Usage of method implementations:
/// <code>
/// public class Migrations : DataMigration
/// {
///     public static int Create() { return 1; } // or
///     public Task&lt;int&gt; CreateAsync() { return 1; }
///     public static int UpdateFrom1() { return 2; } // or
///     public Task&lt;int&gt; UpdateFrom1Async() { return 2; }
///     public static void Uninstall() { } // or
///     public Task UninstallAsync() { }
/// }
/// </code>
/// </example>
public abstract class DataMigration : IDataMigration
{
    /// <inheritdocs />
    public ISchemaBuilder SchemaBuilder { get; set; }

    /// <summary>
    /// Gets or sets whether the current step creates the indexes it adds to a table that existed before the step.
    /// </summary>
    /// <remarks>
    /// <para>The migration manager sets it from <see cref="DataMigrationOptions.CreateIndexesInUpdates"/> before the update steps
    /// of an existing migration. It is always <see langword="true"/> for the <c>Create</c> step and the update steps that run
    /// right after it, as the tables are new and empty then.</para>
    /// <para>An update step that adds an index to an existing table checks it first, and the index is listed with the SQL to
    /// create it in the release notes of its version. An index on a table created in the same step doesn't check it.</para>
    /// </remarks>
    public bool CreateIndexesInUpdates { get; set; } = true;
}
