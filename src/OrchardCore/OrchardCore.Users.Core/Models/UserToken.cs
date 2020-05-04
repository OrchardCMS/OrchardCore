namespace OrchardCore.Users.Models
{
    public class UserToken
    {
        public string LoginProvider { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
