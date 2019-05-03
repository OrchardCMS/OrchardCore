using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    public interface IUserCreatedEventDisplay
    {
        Task CreatedAsync(CreateUserContext context);
    }
}