namespace OrchardCore.Users.Models;

public class TwoFactorMethod
{
    public string Provider { get; set; }

    public bool IsEnabled { get; set; }
}
