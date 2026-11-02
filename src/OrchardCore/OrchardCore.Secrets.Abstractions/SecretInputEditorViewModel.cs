namespace OrchardCore.Secrets;

/// <summary>
/// The model of the <c>SecretInput</c> partial view that the Secrets module provides to render a
/// <see cref="SecretInputViewModel"/> when it is enabled.
/// </summary>
public sealed class SecretInputEditorViewModel
{
    /// <summary>
    /// Gets or sets the edited credential.
    /// </summary>
    public SecretInputViewModel Input { get; set; }

    /// <summary>
    /// Gets or sets the full HTML field name of the credential.
    /// </summary>
    public string HtmlName { get; set; }

    /// <summary>
    /// Gets or sets the HTML id of the credential, given to its first control so that a label targeting the property selects it.
    /// </summary>
    public string HtmlId { get; set; }

    /// <summary>
    /// Gets or sets the name suggested when the user creates a secret for this credential, for instance <c>Facebook.AppSecret</c>.
    /// </summary>
    public string SuggestedSecretName { get; set; }

    /// <summary>
    /// Gets or sets the placeholder of the value input.
    /// </summary>
    public string Placeholder { get; set; }
}
