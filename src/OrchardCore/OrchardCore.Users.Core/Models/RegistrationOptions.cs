namespace OrchardCore.Users.Models;

public class RegistrationOptions
{
    public bool UsersMustValidateEmail { get; set; }

    public bool UsersAreModerated { get; set; }

    public bool UseSiteTheme { get; set; }
}
