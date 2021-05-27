namespace OrchardCore.Users.Handlers
{
    /// <summary>
    /// Represents a context for updating a user.
    /// </summary>
    public class UserUpdateContext : UserContextBase
    {
        /// <inheritdocs />
        public UserUpdateContext(IUser user) : base(user)
        {
        }

        public bool Cancel { get; set; }
    }
}
