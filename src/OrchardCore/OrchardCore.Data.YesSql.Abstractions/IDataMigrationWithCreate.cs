namespace OrchardCore.Data.Migration;

/// <summary>
/// Use this interface to declare that your <see cref="IDataMigration"/> type has a <see cref="Create"/> method. This
/// prevents false-positive <c>CA1822</c> errors and improves performance.
/// </summary>
public interface IDataMigrationWithCreate : IDataMigration
{
    /// <summary>
    /// The method called when the migration is first used.
    /// </summary>
    int Create();
}
