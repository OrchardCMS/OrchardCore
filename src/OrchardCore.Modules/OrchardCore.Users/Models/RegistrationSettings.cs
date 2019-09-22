namespace OrchardCore.Users.Models
{
    public class RegistrationSettings
    {
        public enum UsersCanRegisterEnum
        {
            NoRegistration = 0,
            AllowRegistration = 1,
            AllowOnlyExternalUsers = 2
        }

        public UsersCanRegisterEnum UsersCanRegister { get; set; }
        public bool UsersMustValidateEmail { get; set; }
        public bool UseSiteTheme { get; set; }      
        public bool NoPasswordForExternalUsers { get; set; }
        public bool NoUsernameForExternalUsers { get; set; }
        public bool NoEmailForExternalUsers { get; set; }
        public string GenerateUsernameScript { get; set; }
    }
}
