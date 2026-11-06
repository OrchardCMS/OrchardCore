using OrchardCore.Admin;

namespace OrchardCore.OpenId.ViewModels;

public class OpenIdClientRegistrationIndexViewModel
{
    public string SearchText { get; set; }

    public AdminListContext List { get; set; }
}
