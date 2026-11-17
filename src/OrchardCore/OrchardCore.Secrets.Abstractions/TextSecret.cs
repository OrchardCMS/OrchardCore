namespace OrchardCore.Secrets;

/// <summary>
/// A simple text-based secret for storing sensitive string values, one of the cases of <see cref="Secret"/>.
/// </summary>
public sealed class TextSecret
{
    /// <summary>
    /// Gets or sets the secret text value.
    /// </summary>
    public string Text { get; set; }
}
