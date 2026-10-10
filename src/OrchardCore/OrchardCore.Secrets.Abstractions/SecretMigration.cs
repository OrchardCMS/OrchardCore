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
    private readonly List<Func<Task>> _settingsUpdates = [];

    /// <summary>
    /// Gets or sets the name of the store that receives the new secrets.
    /// </summary>
    public string Store { get; set; }

    /// <summary>
    /// Gets the outcome of each credential that was selected.
    /// </summary>
    public IList<SecretMigrationResult> Results { get; } = [];

    /// <summary>
    /// Updates the settings of the credentials that were moved, so that they reference their new secrets.
    /// </summary>
    /// <remarks>
    /// The Migrate Secrets screen calls this once every display driver saved its secrets. A secret store can commit a
    /// secret in its own scope, which must not wait on settings that the same request already changed.
    /// </remarks>
    public async Task UpdateSettingsAsync()
    {
        foreach (var update in _settingsUpdates)
        {
            await update();
        }

        _settingsUpdates.Clear();
    }

    internal void AddSettingsUpdate(Func<Task> update)
        => _settingsUpdates.Add(update);
}
