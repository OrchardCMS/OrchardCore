using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels;

public class TwoFactorAuthenticationViewModel
{
    public bool HasAuthenticator { get; set; }

    public bool IsTwoFaEnabled { get; set; }

    public bool IsMachineRemembered { get; set; }

    public int RecoveryCodesLeft { get; set; }

    [BindNever]
    public bool CanDisableTwoFa { get; set; }
}
