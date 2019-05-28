namespace OrchardCore.Users.Models
{
    public class LoginSettings
    {
        public bool UseSiteTheme { get; set; }

        public bool SyncRolesFromExternalProviders { get; set; }

        public string GetRolesScript { get; set; }
    }
}
