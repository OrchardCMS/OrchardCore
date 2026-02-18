using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Scope;
using YesSql.Sql;

namespace OrchardCore.Data.Migration;

/// <summary>
/// Represents a contract for a database migration.
/// </summary>
[FeatureTypeDiscovery(SingleFeatureOnly = true, SkipExtension = true)]
public interface IDataMigration
{
    /// <summary>
    /// Gets a value indicating whether the <c>Create()</c> method should be skipped during initialization. This way it
    /// will only run for existing tenants.
    /// </summary>
    bool SkipIfInitializing { get; }

    /// <summary>
    /// Gets the value which should be if the <c>Create()</c> method is skipped during new tenant creation.
    /// </summary>
    int SkipIfInitializingReturnValue { get; }
    
    /// <summary>
    /// Gets or sets the database schema builder.
    /// </summary>
    ISchemaBuilder SchemaBuilder { get; set; }
}
