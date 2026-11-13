namespace OrchardCore.Users.ViewModels;

public class PasskeyLoginViewModel
{
    /// <summary>
    /// Gets or sets the credential returned by <c>navigator.credentials.get()</c>, serialized as JSON.
    /// </summary>
    public string Credential { get; set; }

    public bool RememberMe { get; set; }
}
