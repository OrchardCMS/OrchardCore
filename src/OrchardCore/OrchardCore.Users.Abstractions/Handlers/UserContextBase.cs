namespace OrchardCore.Users.Handlers
{
    public class UserContextBase
    {
        protected UserContextBase(IUser user)
        {
            User = user;
        }

        public IUser User { get; private set; }
    }
}