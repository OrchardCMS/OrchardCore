using OrchardCore.DisplayManagement;

namespace OrchardCore.Users.Models;

/// <summary>
/// Model for user registration confirmation email with source-generated Arguments provider.
/// </summary>
[GenerateArgumentsProvider]
public partial class UserRegistrationEmailModel
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string ConfirmationUrl { get; set; }
    public string SiteName { get; set; }
}
