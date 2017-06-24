using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Orchard.Events;
using Orchard.Users.Models;

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

            var superUser = new User
            {
                UserName = userName,
                Email = email,
                RoleNames = { "Administrator" }
            };

            return userService.CreateUserAsync(superUser,password,reportError);
        }
    }
}