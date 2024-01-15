using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace OrchardCore.Email;

/// <summary>
/// Represents a settings for an email.
/// </summary>
public class EmailSettings : IAsyncOptions
{
    /// <summary>
    /// Gets or sets the default sender mail.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    public string DefaultSender { get; set; }
}
