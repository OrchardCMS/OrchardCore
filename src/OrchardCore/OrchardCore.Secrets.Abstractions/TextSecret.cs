namespace OrchardCore.Secrets;

/// <summary>
/// A simple text-based secret for storing sensitive string values.
/// </summary>
public class TextSecret : SecretBase
{
    /// <summary>
    /// Gets or sets the secret text value.
    /// </summary>
    public string Text { get; set; }
}
