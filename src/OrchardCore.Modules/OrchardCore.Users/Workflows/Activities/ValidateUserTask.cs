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
        private readonly IStringLocalizer S;
        private readonly IRoleService _roleService;

        public ValidateUserTask(UserManager<IUser> userManager, IHttpContextAccessor httpContextAccessor, IStringLocalizer<ValidateUserTask> localizer, IRoleService roleService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _roleService = roleService;
            S = localizer;
        }

        public override string Name => nameof(ValidateUserTask);

        public override LocalizedString Category => S["User"];

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

        public override LocalizedString DisplayText => S["Validate User Task"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Anonymous"], S["Authenticated"], S["InRole"]);
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

                if (Roles.Any())
                {
                    var userRoleNames = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(user));
                    foreach (var role in Roles)
                    {
                        if (userRoleNames.Contains(role))
                        {
                            workflowContext.LastResult = userRoleNames;
                            return Outcomes("InRole");
                        }
                    }
                }

                return Outcomes("Authenticated");
            }

            return Outcomes("Anonymous");
        }
    }
}
