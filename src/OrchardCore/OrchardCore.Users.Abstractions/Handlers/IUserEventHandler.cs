using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    /// <summary>
    /// Contract for user events.
    /// </summary>
    public interface IUserEventHandler
    {
        /// <summary>
        /// Occurs when a user is created.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/>.</param>
        Task CreatedAsync(UserContext context) => Task.CompletedTask;

        /// <summary>
        /// Occurs when a user is deleted.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/>.</param>
        Task DeletedAsync(UserContext context) => Task.CompletedTask;

        /// <summary>
        /// Occurs when a user is disabled.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/>.</param>
        Task DisabledAsync(UserContext context) => Task.CompletedTask;

        /// <summary>
        /// Occurs when a user is enabled.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/>.</param>
        Task EnabledAsync(UserContext context) => Task.CompletedTask;

        /// <summary>
        /// Occurs when a user is updated.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/>.</param>
        Task UpdatedAsync(UserContext context) => Task.CompletedTask;
    }
}
