namespace OrchardCore.Sms;

public class SmsMessage
{
    /// <summary>
    /// The phone number to send the message from.  
    /// If not specified, the provider's default phone number will be used.  
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// The phone number to send the message to.
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// The body of the message to send.
    /// </summary>
    public string Body { get; set; }
}
