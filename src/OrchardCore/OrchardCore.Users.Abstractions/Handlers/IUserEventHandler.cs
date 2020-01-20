using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    public interface IUserEventHandler
    {
        Task CreatedAsync(UserContext context);

        Task DisabledAsync(UserContext context);

        Task EnabledAsync(UserContext context);
    }
}
