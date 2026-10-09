namespace OrchardCore.Secrets;

/// <summary>
/// The model of the screen of the Secrets module that moves credentials kept in the settings of other features to secrets.
/// </summary>
/// <remarks>
/// A feature that keeps credentials in its settings contributes a <c>DisplayDriver&lt;SecretMigration&gt;</c>. Its editor
/// renders one <c>SecretMigrationItem_Edit</c> shape bound to a <see cref="SecretMigrationItemViewModel"/> per credential that
/// is kept in the settings, and its update moves the selected ones with
/// <c>MoveToSecretAsync()</c> of <see cref="SecretMigrationExtensions"/>.
/// </remarks>
public sealed class SecretMigration
{
    /// <summary>
    /// Gets or sets the name of the store that receives the new secrets.
    /// </summary>
    public string Store { get; set; }

    /// <summary>
    /// Gets the outcome of each credential that was selected.
    /// </summary>
    public IList<SecretMigrationResult> Results { get; } = [];
}
