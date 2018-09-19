using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Users.Workflows.Activities
{
    public class RegisterUserTask : TaskActivity
    {
        private readonly IUserService _userService;
        private readonly IStringLocalizer T;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public RegisterUserTask(IUserService userService, IHttpContextAccessor httpContextAccessor, IUpdateModelAccessor updateModelAccessor, IStringLocalizer<RegisterUserTask> t)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _updateModelAccessor = updateModelAccessor;
            T = t;
        }

        // The technical name of the activity. Activities on a workflow definition reference this name.
        public override string Name => nameof(RegisterUserTask);

        // The category to which this activity belongs. The activity picker groups activities by this category.
        public override LocalizedString Category => T["Content"];

        // Returns the possible outcomes of this activity.
        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"], T["Valid"], T["Invalid"]);
        }

        // This is the heart of the activity and actually performs the work to be done.
        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {

            var form = _httpContextAccessor.HttpContext.Request.Form;
            var email = form["Email"];
            var isValid = !string.IsNullOrWhiteSpace(email);
            var outcome = isValid ? "Valid" : "Invalid";


            if (isValid)
            {
                var userName = form["UserName"];
                if (string.IsNullOrWhiteSpace(userName))
                    userName = email;

                var errors = new Dictionary<string, string>();
                var user = await _userService.CreateUserAsync(new User() { UserName = userName, Email = email }, null, (key, message) => errors.Add(key, message));
                if (errors.Count > 0)
                {
                    var updater = _updateModelAccessor.ModelUpdater;
                    if (updater != null)
                    {
                        foreach (var item in errors)
                        {
                            updater.ModelState.TryAddModelError(item.Key, T[item.Value]);
                        }
                    }
                    outcome = "Invalid";
                }
            }

            return Outcomes("Done", outcome);
        }
    }
}
