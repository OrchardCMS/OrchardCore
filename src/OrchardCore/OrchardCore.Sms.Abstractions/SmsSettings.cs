namespace OrchardCore.Sms;

public class SmsSettings
{
    /// <summary>
    /// The group identifier for configuring drivers.
    /// </summary>
    public const string GroupId = "sms";

    /// <summary>
    /// The name of the SMS provider to use.
    /// </summary>
    public string DefaultProviderName { get; set; }
}
