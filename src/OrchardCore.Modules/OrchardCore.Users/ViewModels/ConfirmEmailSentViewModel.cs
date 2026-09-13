namespace OrchardCore.Users.ViewModels;

public sealed class ConfirmEmailSentViewModel
{
    public string ReturnUrl { get; set; }

    public bool CanResendEmailConfirmation { get; set; }
}
