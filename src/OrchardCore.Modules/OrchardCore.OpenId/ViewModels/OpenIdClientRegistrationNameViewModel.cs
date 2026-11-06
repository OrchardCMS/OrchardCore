using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.OpenId.ViewModels;

public class OpenIdClientRegistrationNameViewModel
{
    public string Name { get; set; }

    [BindNever]
    public bool IsNew { get; set; }
}
