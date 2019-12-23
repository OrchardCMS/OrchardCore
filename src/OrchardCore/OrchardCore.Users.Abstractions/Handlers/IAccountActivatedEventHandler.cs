using System;
using System.Threading.Tasks;

namespace OrchardCore.Users.Handlers
{
    public interface IAccountActivatedEventHandler
    {
        Task AccountActivatedAsync(AccountActivatedContext context);
    }
}
