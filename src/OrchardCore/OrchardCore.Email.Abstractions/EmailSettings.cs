namespace OrchardCore.Email;

public class EmailSettings
{
    /// <summary>
    /// The group identifier for configuring drivers.
    /// </summary>
    public const string GroupId = "email";

    /// <summary>
    /// The name of the email provider to use by default.
    /// </summary>
    public string DefaultProviderName { get; set; }
}
