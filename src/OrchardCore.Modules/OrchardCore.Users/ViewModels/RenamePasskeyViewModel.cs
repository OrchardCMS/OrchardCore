using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels;

public class RenamePasskeyViewModel
{
    public const int MaxNameLength = 100;

    /// <summary>
    /// Gets or sets the credential identifier, encoded with Base64Url.
    /// </summary>
    [Required]
    public string Id { get; set; }

    [Required]
    [StringLength(MaxNameLength)]
    public string Name { get; set; }
}
