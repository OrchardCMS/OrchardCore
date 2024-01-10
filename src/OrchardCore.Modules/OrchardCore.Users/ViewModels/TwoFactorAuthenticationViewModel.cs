using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Users.ViewModels;

public class TwoFactorAuthenticationViewModel
{
    [Required]
    public string PreferredProvider { get; set; }

    [BindNever]
    public bool IsTwoFaEnabled { get; set; }

    [BindNever]
    public bool IsMachineRemembered { get; set; }

    [BindNever]
    public int RecoveryCodesLeft { get; set; }

    [BindNever]
    public bool CanDisableTwoFactor { get; set; }

    [BindNever]
    public IList<IShape> AuthenticationMethods { get; } = new List<IShape>();

    [BindNever]
    public IUser User { get; set; }

    [BindNever]
    public IList<SelectListItem> ValidTwoFactorProviders { get; set; }
}
