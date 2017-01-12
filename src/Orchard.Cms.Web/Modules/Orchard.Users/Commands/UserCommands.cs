using Microsoft.Extensions.Localization;
using Orchard.Environment.Commands;
using Orchard.Users.Models;
using Orchard.Users.Services;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Users.Commands
{
    public class UserCommands : DefaultCommandHandler
    {
        private readonly IUserService _userService;
        
        public UserCommands(IUserService userService,
                            IStringLocalizer<UserCommands> localizer) : base(localizer)
        {
            _userService = userService;
        }

        [OrchardSwitch]
        public string UserName { get; set; }

        [OrchardSwitch]
        public string Password { get; set; }

        [OrchardSwitch]
        public string Email { get; set; }

        [OrchardSwitch]
        public string Roles { get; set; }

        [CommandName("createUser")]
        [CommandHelp("createUser /UserName:<username> /Password:<password> /Email:<email> /Roles:{rolename,rolename,...}\r\n\t" + "Creates a new User")]
        [OrchardSwitches("UserName,Password,Email,Roles")]
        public void CreateUser()
        {
            var user = new User
            {
                UserName = UserName,
                Email = Email,
                RoleNames = (Roles ?? "").Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList()
            };
            if (_userService.CreateUserAsync(user, Password, (key, message) => Context.Output.WriteLine(message)).Result)
                Context.Output.WriteLine(T["User created successfully"]);
        }
    }
}