using System;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Commands;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Commands
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
            var roleNames = (Roles ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            var valid = true;

            _userService.CreateUserAsync(new User { UserName = UserName, Email = Email, RoleNames = roleNames, EmailConfirmed = true }, Password, (key, message) =>
            {
                valid = false;
                Context.Output.WriteLine(message);
            }).GetAwaiter().GetResult();

            if (valid)
            {
                Context.Output.WriteLine(T["User created successfully"]);
            }
        }
    }
}