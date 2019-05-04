using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    public interface IUserCreatedEventHandler
    {
        Task CreatedAsync(CreateUserContext context);
    }
}