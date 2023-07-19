using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Users.Workflows.Activities
{
    public class ValidateUserTask : TaskActivity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _roleClaimType;
        protected readonly IStringLocalizer S;

        public ValidateUserTask(IHttpContextAccessor httpContextAccessor, IOptions<IdentityOptions> optionsAccessor, IStringLocalizer<ValidateUserTask> localizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _roleClaimType = optionsAccessor.Value.ClaimsIdentity.RoleClaimType;
            S = localizer;
        }

        public override string Name => nameof(ValidateUserTask);

        public override LocalizedString Category => S["User"];

        public bool SetUserName
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

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var user = _httpContextAccessor.HttpContext.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated;

            if (isAuthenticated == true)
            {
                if (SetUserName)
                {
                    workflowContext.Properties["UserName"] = user.Identity.Name;
                }

                if (Roles.Any())
                {
                    var userRoleNames = user
                        .FindAll(c => c.Type == _roleClaimType)
                        .Select(c => c.Value)
                        .ToList();

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
