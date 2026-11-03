namespace OrchardCore.OpenId.ViewModels;

public class VerifyViewModel
{
    public const string ResultParameter = "result";
    public const string ApprovedResult = "approved";
    public const string DeniedResult = "denied";

    public string ApplicationName { get; set; }

    public string Error { get; set; }

    public string ErrorDescription { get; set; }

    public string Scope { get; set; }

    public string UserCode { get; set; }

    /// <summary>
    /// Gets or sets the outcome of the verification the user just completed, if any.
    /// </summary>
    public string Result { get; set; }
}
