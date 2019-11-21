namespace OrchardCore.Users.Handlers
{
    public class ExternalUserContext : UserContextBase
    {
        public ExternalUserContext(IUser user) : base(user)
        {
        }
    }
}