using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;

namespace OrchardCore.Users.ViewModels
{
    public class EditUserViewModel
    {
        public bool EmailConfirmed { get; set; }

        public bool IsEnabled { get; set; }

        public RoleViewModel[] Roles { get; set; }
    }
}
