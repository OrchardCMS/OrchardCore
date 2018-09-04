using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    public interface IRegistrationEvents
    {
        Task Registering();

        Task Registered();
    }
}