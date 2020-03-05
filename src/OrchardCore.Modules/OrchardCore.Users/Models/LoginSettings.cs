namespace OrchardCore.Users.Models
{
    public class LoginSettings
    {
        public bool UseSiteTheme { get; set; }

        public bool UseExternalProviderIfOnlyOneDefined { get; set; }

        public bool DisableLocalLogin { get; set; }

        public bool UseScriptToSyncRoles { get; set; }

        public string SyncRolesScript { get; set; }
    }
}
