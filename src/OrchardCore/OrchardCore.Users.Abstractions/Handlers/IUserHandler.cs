using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    public interface IUserHandler
    {
        Task CreatedAsync(CreateUserContext context);
    }
}