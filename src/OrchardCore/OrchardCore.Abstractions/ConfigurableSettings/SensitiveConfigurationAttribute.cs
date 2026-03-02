namespace OrchardCore.Settings;

/// <summary>
/// Marks a property as containing sensitive information that should be masked in the Admin UI.
/// </summary>
/// <remarks>
/// Apply this attribute to properties that contain secrets like API keys, passwords, or tokens.
/// The Admin UI will display masked values and may provide additional security measures.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class SensitiveConfigurationAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the character used to mask the sensitive value.
    /// Defaults to '•' (bullet).
    /// </summary>
    public char MaskCharacter { get; set; } = '•';

    /// <summary>
    /// Gets or sets the number of characters to show unmasked at the end of the value.
    /// Defaults to 4.
    /// </summary>
    /// <remarks>
    /// Set to 0 to mask the entire value.
    /// </remarks>
    public int VisibleCharacters { get; set; } = 4;

    /// <summary>
    /// Gets or sets whether the full value can be revealed in the UI via a toggle.
    /// Defaults to <c>false</c> for maximum security.
    /// </summary>
    public bool AllowReveal { get; set; }
}
