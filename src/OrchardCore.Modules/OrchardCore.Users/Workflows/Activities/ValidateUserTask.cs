using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using OrchardCore.Security.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Users.Workflows.Activities
{
    public class ValidateUserTask : TaskActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IUser> _userManager;
        private readonly IStringLocalizer T;
        private readonly IRoleService _roleService;

        public ValidateUserTask(UserManager<IUser> userManager, IHttpContextAccessor httpContextAccessor, IStringLocalizer<ValidateUserTask> localizer, IRoleService roleService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _roleService = roleService;
            T = localizer;
        }

        public override string Name => nameof(ValidateUserTask);

        public override LocalizedString Category => T["User"];

        public bool SetUser
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public IEnumerable<string> Roles
        {
            get => GetProperty(() => new List<string>());
            set => SetProperty(value);
        }

        public override LocalizedString DisplayText => T["Validate User Task"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var outcomes = Roles.Select(x => T[x]).ToList();
            outcomes.Add(T["Authenticated"]);
            outcomes.Add(T["Anonymous"]);

            return Outcomes(outcomes.ToArray());
        }

        public async override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var user = _httpContextAccessor.HttpContext.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated;

            if (isAuthenticated == true)
            {
                if (SetUser)
                {
                    workflowContext.Properties["User"] = user;
                }

                var outcomes = new List<string>
                {
                    "Authenticated"
                };

                if (Roles.Any())
                {
                    var userRoleNames = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(user));
                    foreach (var role in Roles)
                    {
                        if (userRoleNames.Contains(role, StringComparer.CurrentCultureIgnoreCase))
                        {
                            outcomes.Add(role);
                        }
                    }
                }

                return Outcomes(outcomes.ToArray());
            }

            return Outcomes("Anonymous");
        }
    }
}