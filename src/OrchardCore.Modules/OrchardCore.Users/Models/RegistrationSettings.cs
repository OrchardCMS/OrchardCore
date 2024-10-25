namespace OrchardCore.Users.Models;

public class RegistrationSettings
{
    [Obsolete("This property is no longer used and will be removed in the next major release.")]
    public UserRegistrationType UsersCanRegister { get; set; }

    public bool UsersMustValidateEmail { get; set; }

    public bool UsersAreModerated { get; set; }

    public bool UseSiteTheme { get; set; }

    [Obsolete("This property is no longer used and will be removed in the next major release. Instead use ExternalAuthenticationSettings.NoPassword")]
    public bool NoPasswordForExternalUsers { get; set; }

    [Obsolete("This property is no longer used and will be removed in the next major release. Instead use ExternalAuthenticationSettings.NoUsername")]
    public bool NoUsernameForExternalUsers { get; set; }

    [Obsolete("This property is no longer used and will be removed in the next major release. Instead use ExternalAuthenticationSettings.NoEmail")]
    public bool NoEmailForExternalUsers { get; set; }

    [Obsolete("This property is no longer used and will be removed in the next major release. Instead use ExternalAuthenticationSettings.UseScriptToGenerateUsername")]
    public bool UseScriptToGenerateUsername { get; set; }

    [Obsolete("This property is no longer used and will be removed in the next major release. Instead use ExternalAuthenticationSettings.GenerateUsernameScript")]
    public string GenerateUsernameScript { get; set; }
}
