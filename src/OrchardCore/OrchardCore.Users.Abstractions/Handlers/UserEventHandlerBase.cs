using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    public abstract class UserEventHandlerBase : IUserEventHandler
    {
        /// <inheritdocs />
        public virtual Task CreatingAsync(UserCreateContext context) => Task.CompletedTask;

        /// <inheritdocs />
        public virtual Task CreatedAsync(UserCreateContext context) => Task.CompletedTask;

        /// <inheritdocs />
        public virtual Task DeletingAsync(UserDeleteContext context) => Task.CompletedTask;

        /// <inheritdocs />
        public virtual Task DeletedAsync(UserDeleteContext context) => Task.CompletedTask;

        /// <inheritdocs />
        public virtual Task UpdatingAsync(UserUpdateContext context) => Task.CompletedTask;

        /// <inheritdocs />
        public virtual Task UpdatedAsync(UserUpdateContext context) => Task.CompletedTask;

        /// <inheritdocs />
        public virtual Task DisabledAsync(UserContext context) => Task.CompletedTask;

        /// <inheritdocs />
        public virtual Task EnabledAsync(UserContext context) => Task.CompletedTask;
    }
}
