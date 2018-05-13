using System;
using System.Threading.Tasks;
using OrchardCore.Setup.Events;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// During setup, creates the admin user account.
    /// </summary>
    public class SetupEventHandler : ISetupEventHandler
    {
        private readonly IUserService _userService;

        public SetupEventHandler(IUserService userService)
        {
            _userService = userService;
        }

        public Task Setup(
            string siteName,
            string userName,
            string email,
            string password,
            string dbProvider,
            string dbConnectionString,
            string dbTablePrefix,
            string siteTimeZone,
            Action<string, string> reportError
            )
        {
            return _userService.CreateUserAsync(userName, email, new string[] { "Administrator" }, password, reportError);
        }
    }
}