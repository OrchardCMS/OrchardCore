namespace OrchardCore.Data.Migration;

/// <summary>
/// Defines in which environments the update steps of the data migrations create the indexes they add to existing tables.
/// </summary>
/// <remarks>
/// The <c>Create</c> step of a migration, and the update steps that run right after it, always create their indexes, as the
/// tables are new and empty then.
/// </remarks>
public enum MigrationIndexCreation
{
    /// <summary>
    /// The update steps create their indexes in every environment.
    /// </summary>
    All,

    /// <summary>
    /// The update steps create their indexes only when the application runs in the <c>Development</c> environment. In other
    /// environments, the indexes are left out, to be created with the SQL listed in the release notes.
    /// </summary>
    Development,

    /// <summary>
    /// The update steps never create their indexes, to be created with the SQL listed in the release notes.
    /// </summary>
    Disabled,
}
