namespace OrchardCore.Users.Handlers
{
    /// <summary>
    /// Represents a context for a user creation.
    /// </summary>
    public class UserCreateContext : UserContextBase
    {
        /// <inheritdocs />
        public UserCreateContext(IUser user) : base(user)
        {
        }

        public bool Cancel { get; set; }
    }
}
