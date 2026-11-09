namespace OrchardCore.Data.Migration;

/// <summary>
/// Configures how the data migrations run, from the <c>OrchardCore:Data:Migrations</c> configuration section.
/// See <see href="https://docs.orchardcore.net/en/latest/reference/modules/Migrations/#indexes-added-by-update-steps" />.
/// </summary>
public sealed class DataMigrationOptions
{
    /// <summary>
    /// Gets or sets in which environments the update steps of the data migrations create the indexes they add to existing
    /// tables. Defaults to <see cref="MigrationIndexCreation.All"/>.
    /// </summary>
    /// <remarks>
    /// Creating an index on a large table can take a long time, and on most databases blocks the writes to the table meanwhile.
    /// When the update steps don't create them, the indexes can be created later with the SQL listed in the release notes, using the
    /// online options of the database.
    /// </remarks>
    public MigrationIndexCreation CreateIndexesInUpdates { get; set; } = MigrationIndexCreation.All;
}
