namespace OrchardCore.Users.Models
{
    public class LoginSettings
    {
        public bool UseSiteTheme { get; set; }

        public bool DisableLocalUsersAuthentication { get; set; }

        public string MapExternalUsersScript { get; set; }
    }
}
