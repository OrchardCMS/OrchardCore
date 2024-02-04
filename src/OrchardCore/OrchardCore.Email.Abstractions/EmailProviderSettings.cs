using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Email;

public class EmailProviderSettings
{
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the default sender mail.
    /// </summary>
    [Required(AllowEmptyStrings = false), EmailAddress]
    public string DefaultSender { get; set; }
}
