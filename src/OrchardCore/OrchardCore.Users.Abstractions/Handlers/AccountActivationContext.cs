
namespace OrchardCore.Users.Handlers
{
    public class AccountActivationContext : UserContextBase
    {
        public AccountActivationContext(IUser user) : base(user)
        {
        }

        public string ActivationUrl { get; set; }
    }
}
