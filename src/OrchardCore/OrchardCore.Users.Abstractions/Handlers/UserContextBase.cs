namespace OrchardCore.Users.Handlers
{
    /// <summary>
    /// Represents a base context for a user.
    /// </summary>
    public class UserContextBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="UserContextBase"/>.
        /// </summary>
        /// <param name="user">The <see cref="IUser"/>.</param>
        protected UserContextBase(IUser user)
        {
            User = user;
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        public IUser User { get; private set; }
    }
}
