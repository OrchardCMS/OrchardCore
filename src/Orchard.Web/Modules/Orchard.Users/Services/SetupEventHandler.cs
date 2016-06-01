using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Events;
using Orchard.Users.Models;

namespace Orchard.Users.Services
{
    public interface ISetupEventHandler : IEventHandler
    {
        Task CreateSuperUserAsync(string userName, string email, string password);
    }

    public class SetupEventHandler : ISetupEventHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public SetupEventHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task CreateSuperUserAsync(string userName, string email, string password)
        {
            var userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
            var superUser = new User
            {
                UserName = userName,
                Email = email,
            };

            return userManager.CreateAsync(superUser, password);

        }
    }
}
