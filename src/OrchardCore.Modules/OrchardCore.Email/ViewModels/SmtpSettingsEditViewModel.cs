namespace OrchardCore.Email.ViewModels
{
    public class SmtpSettingsEditViewModel : SmtpSettings
    {
        /// <summary>
        /// Gets or sets the user password.
        /// </summary>
        public new string Password { get; set; }
    }
}
