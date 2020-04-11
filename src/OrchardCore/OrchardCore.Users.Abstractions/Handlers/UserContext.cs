namespace OrchardCore.Users.Handlers
{
    /// <summary>
    /// Represents a context for a user.
    /// </summary>
    public class UserContext : UserContextBase
    {
        /// <inheritdocs />
        public UserContext(IUser user) : base(user)
        {
        }
    }
}
