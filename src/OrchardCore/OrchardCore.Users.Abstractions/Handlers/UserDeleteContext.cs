namespace OrchardCore.Users.Handlers
{
    /// <summary>
    /// Represents a context for deleting a user.
    /// </summary>
    public class UserDeleteContext : UserContextBase
    {
        /// <inheritdocs />
        public UserDeleteContext(IUser user) : base(user)
        {
        }

        public bool Cancel { get; set; }
    }
}
