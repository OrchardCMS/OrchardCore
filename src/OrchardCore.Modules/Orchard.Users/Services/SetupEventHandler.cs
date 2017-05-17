using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Events;

namespace Orchard.Users.Services
{
    public interface ISetupEventHandler : IEventHandler
    {
        Task Setup(string userName, string email, string password, Action<string, string> reportError);
    }

    /// <summary>
    /// During setup, creates the admin user account.
    /// </summary>
    public class SetupEventHandler : ISetupEventHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public SetupEventHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task Setup(string userName, string email, string password, Action<string, string> reportError)
        {
            var userService = _serviceProvider.GetRequiredService<IUserService>();
            return userService.CreateUserAsync(userName, email, new string [] { "Administrator" }, password, reportError);
        }
    }
}