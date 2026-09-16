namespace OrchardCore.OpenId.ViewModels;

public class VerifyViewModel
{
    public string ApplicationName { get; set; }

    public string Error { get; set; }

    public string ErrorDescription { get; set; }

    public string Scope { get; set; }

    public string UserCode { get; set; }
}
