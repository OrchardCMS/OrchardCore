namespace OrchardCore.DisplayManagement.Notify;

public sealed class NotifyContext
{
    /// <summary>
    /// Total milliseconds to auto dismiss the alert. Keep null to not auto dismiss.
    /// </summary>
    public int? DismissalMilliseconds { get; set; }
}
