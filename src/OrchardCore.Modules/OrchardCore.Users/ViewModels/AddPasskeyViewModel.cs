using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels;

public class AddPasskeyViewModel
{
    /// <summary>
    /// Gets or sets the credential returned by <c>navigator.credentials.create()</c>, serialized as JSON.
    /// </summary>
    [Required]
    public string Credential { get; set; }

    [StringLength(RenamePasskeyViewModel.MaxNameLength)]
    public string Name { get; set; }
}
