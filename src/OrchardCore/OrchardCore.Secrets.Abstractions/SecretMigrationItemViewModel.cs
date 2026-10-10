using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Secrets;

/// <summary>
/// The model of the <c>SecretMigrationItem_Edit</c> shape, which lets the user select a credential to move to a secret.
/// </summary>
public class SecretMigrationItemViewModel
{
    /// <summary>
    /// Gets or sets whether the credential should be moved.
    /// </summary>
    public bool Migrate { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret to create.
    /// </summary>
    public string SecretName { get; set; }

    /// <summary>
    /// Gets or sets the name of the feature that uses the credential, for instance <c>Meta</c>.
    /// </summary>
    [BindNever]
    public string Group { get; set; }

    /// <summary>
    /// Gets or sets the name of the credential, for instance <c>App secret</c>.
    /// </summary>
    [BindNever]
    public string DisplayName { get; set; }
}
