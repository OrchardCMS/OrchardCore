namespace OrchardCore.Users.Models;

public class EmailAuthenticatorLoginSettings
{
    public const string DefaultSubject = "Your verification code";

    public const string DefaultBody = "We received your request for a single-use code to use with your Microsoft account. Your code is {{ Code }}";

    public string Subject { get; set; }

    public string Body { get; set; }
}
