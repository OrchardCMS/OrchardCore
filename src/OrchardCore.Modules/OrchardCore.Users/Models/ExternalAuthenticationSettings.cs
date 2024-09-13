namespace OrchardCore.Users.Models;

public class ExternalAuthenticationSettings
{
    public bool NoPassword { get; set; }

    public bool NoUsername { get; set; }

    public bool NoEmail { get; set; }

    public bool UseScriptToGenerateUsername { get; set; }

    public string GenerateUsernameScript { get; set; }
}
