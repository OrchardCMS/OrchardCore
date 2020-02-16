using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    /// <summary>
    /// Contract for user events.
    /// </summary>
    public interface IUserEventHandler
    {
        /// <summary>
        /// Occurs when the user created.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/>.</param>
        Task CreatedAsync(UserContext context);

        /// <summary>
        /// Occurs when the user disabled.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/>.</param>
        Task DisabledAsync(UserContext context);

        /// <summary>
        /// Occurs when the user enabled.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/>.</param>
        Task EnabledAsync(UserContext context);
    }
}
