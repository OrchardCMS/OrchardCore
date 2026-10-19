namespace OrchardCore.Users;

public class UserOptions
{
    private string _loginPath = "Login";
    private string _logoffPath = "Users/LogOff";
    private string _changePasswordUrl = "ChangePassword";
    private string _changePasswordConfirmationUrl = "ChangePasswordConfirmation";
    private string _externalLoginsUrl = "ExternalLogins";
    private string _twoFactorAuthenticationPath = "TwoFactor";
    private string _securityPath = "Security";

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

    /// <summary>
    /// Gets or sets the path of the former two-factor authentication page, which redirects to the <see cref="SecurityPath"/> page.
    /// </summary>
    public string TwoFactorAuthenticationPath
    {
        get => _twoFactorAuthenticationPath;
        set => _twoFactorAuthenticationPath = value.Trim(' ', '/');
    }

    /// <summary>
    /// Gets or sets the path of the page where users manage their two-factor authentication and their passkeys.
    /// </summary>
    public string SecurityPath
    {
        get => _securityPath;
        set => _securityPath = value.Trim(' ', '/');
    }
}
