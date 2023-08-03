namespace OrchardCore.Users.Models;

public class TwoFactorLoginSettings
{
    public bool RequireTwoFactorAuthentication { get; set; }

    public bool AllowRememberClientTwoFactorAuthentication { get; set; }

    public int NumberOfRecoveryCodesToGenerate { get; set; } = 5;
}
