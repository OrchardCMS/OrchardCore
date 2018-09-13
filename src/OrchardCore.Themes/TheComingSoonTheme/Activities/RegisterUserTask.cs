using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace TheComingSoonTheme.Activities
{
    public class RegisterUserTask : TaskActivity
    {
        private readonly IUserService _userService;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;

        public RegisterUserTask(IUserService userService, IStringLocalizer<RegisterUserTask> s, IHtmlLocalizer<RegisterUserTask> h)
        {
            _userService = userService;

            S = s;
            H = h;
        }

        // The technical name of the activity. Activities on a workflow definition reference this name.
        public override string Name => nameof(RegisterUserTask);

        // The category to which this activity belongs. The activity picker groups activities by this category.
        public override LocalizedString Category => S["UI"];

        // The notification type to display.
        public string Email
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        // Returns the possible outcomes of this activity.
        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        // This is the heart of the activity and actually performs the work to be done.
        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {

            IUser user = new User() { UserName = Email, Email = Email };
            Dictionary<string, string> errors = new Dictionary<string, string>();
            user = await _userService.CreateUserAsync(user, null, (key, message) => errors.Add(key, message));

            return Outcomes("Done");
        }
    }
}
