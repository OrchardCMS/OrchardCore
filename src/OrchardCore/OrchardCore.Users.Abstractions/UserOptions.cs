namespace OrchardCore.Users
{
    public class UserOptions
    {
        private string _loginPath = "Login";
        private string _logoffPath = "Users/LogOff";
        private string _changePasswordUrl = "ChangePassword";
        private string _changePasswordConfirmationUrl = "ChangePasswordConfirmation";
        private string _externalLoginsUrl = "ExternalLogins";
        private string _twoFactorAuthenticationPath = "TwoFactor";

        public string LoginPath
        {
            get => _loginPath;
            set => _loginPath = value.Trim(' ', '/');
        }

        public string LogoffPath
        {
            get => _logoffPath;
            set => _logoffPath = value.Trim(' ', '/');
        }

        public string ChangePasswordUrl
        {
            get => _changePasswordUrl;
            set => _changePasswordUrl = value.Trim(' ', '/');
        }

        public string ChangePasswordConfirmationUrl
        {
            get => _changePasswordConfirmationUrl;
            set => _changePasswordConfirmationUrl = value.Trim(' ', '/');
        }

        public string ExternalLoginsUrl
        {
            get => _externalLoginsUrl;
            set => _externalLoginsUrl = value.Trim(' ', '/');
        }

        public string TwoFactorAuthenticationPath
        {
            get => _twoFactorAuthenticationPath;
            set => _twoFactorAuthenticationPath = value.Trim(' ', '/');
        }
    }
}
