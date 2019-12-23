using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    public interface IAccountActivationEventHandler
    {
        Task AccountActivationEventHandler(AccountActivationContext context);
    }
}
