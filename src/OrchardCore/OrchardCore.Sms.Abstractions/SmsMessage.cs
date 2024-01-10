namespace OrchardCore.Sms;

public class SmsMessage
{
    /// <summary>
    /// The phone number to send the message to.
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// The body of the message to send.
    /// </summary>
    public string Body { get; set; }
}
