using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    /// <summary>
    /// Contract for user events.
    /// </summary>
    public interface IUserEventHandler
    {
        /// <summary>
        /// Occurs before a user is created.
        /// </summary>
        /// <param name="context">The <see cref="UserCreateContext"/>.</param>
        Task CreatingAsync(UserCreateContext context);

        /// <summary>
        /// Occurs when a user is created.
        /// </summary>
        /// <param name="context">The <see cref="UserCreateContext"/>.</param>
        Task CreatedAsync(UserCreateContext context);

        /// <summary>
        /// Occurs before a user is deleted.
        /// </summary>
        /// <param name="context">The <see cref="UserDeleteContext"/>.</param>
        Task DeletingAsync(UserDeleteContext context);

        /// <summary>
        /// Occurs when a user is deleted.
        /// </summary>
        /// <param name="context">The <see cref="UserDeleteContext"/>.</param>
        Task DeletedAsync(UserDeleteContext context);

        /// <summary>
        /// Occurs before a user is updated.
        /// </summary>
        /// <param name="context">The <see cref="UserUpdateContext"/>.</param>
        Task UpdatingAsync(UserUpdateContext context);

        /// <summary>
        /// Occurs when a user is updated.
        /// </summary>
        /// <param name="context">The <see cref="UserUpdateContext"/>.</param>
        Task UpdatedAsync(UserUpdateContext context);

        /// <summary>
        /// Occurs when a user is disabled.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/>.</param>
        Task DisabledAsync(UserContext context);

        /// <summary>
        /// Occurs when a user is enabled.
        /// </summary>
        /// <param name="context">The <see cref="UserContext"/>.</param>
        Task EnabledAsync(UserContext context);
    }
}
