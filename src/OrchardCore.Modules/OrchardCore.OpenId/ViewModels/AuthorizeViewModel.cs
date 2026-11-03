namespace OrchardCore.OpenId.ViewModels;

public class AuthorizeViewModel
{
    public string ApplicationName { get; set; }

    public string Scope { get; set; }

    /// <summary>
    /// Gets or sets the host receiving the authorization of a self-registered application, whose name isn't verified.
    /// </summary>
    public string UnverifiedClientHost { get; set; }
}
