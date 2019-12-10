using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Email;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities
{
    public class RegisterUserTask : TaskActivity
    {
        private readonly IUserService _userService;
        private readonly UserManager<IUser> _userManager;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer T;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public RegisterUserTask(IUserService userService, UserManager<IUser> userManager, IWorkflowExpressionEvaluator expressionEvaluator, IHttpContextAccessor httpContextAccessor, IUpdateModelAccessor updateModelAccessor, IStringLocalizer<RegisterUserTask> t)
        {
            _userService = userService;
            _userManager = userManager;
            _expressionEvaluator = expressionEvaluator;
            _httpContextAccessor = httpContextAccessor;
            _updateModelAccessor = updateModelAccessor;
            T = t;
        }

        // The technical name of the activity. Activities on a workflow definition reference this name.
        public override string Name => nameof(RegisterUserTask);
        public override LocalizedString DisplayText => T["Register User Task"];

        // The category to which this activity belongs. The activity picker groups activities by this category.
        public override LocalizedString Category => T["Content"];

        // The message to display.
        public bool SendConfirmationEmail
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ConfirmationEmailSubject
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }


        // The message to display.
        public WorkflowExpression<string> ConfirmationEmailTemplate
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        // Returns the possible outcomes of this activity.
        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"], T["Valid"], T["Invalid"]);
        }

        // This is the heart of the activity and actually performs the work to be done.
        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            bool isValid = false;
            IFormCollection form = null;
            string email = null;
            if (_httpContextAccessor.HttpContext != null)
            {
                form = _httpContextAccessor.HttpContext.Request.Form;
                email = form["Email"];
                isValid = !string.IsNullOrWhiteSpace(email);
            }
            var outcome = isValid ? "Valid" : "Invalid";


            if (isValid)
            {
                var userName = form["UserName"];
                if (string.IsNullOrWhiteSpace(userName))
                    userName = email;

                var errors = new Dictionary<string, string>();
                var user = (User)await _userService.CreateUserAsync(new User() { UserName = userName, Email = email }, null, (key, message) => errors.Add(key, message));
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
                else if (SendConfirmationEmail)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var request = _httpContextAccessor.HttpContext.Request;
                    var uriBuilder = new UriBuilder(request.Scheme, request.Host.Host);
                    if (request.Host.Port.HasValue)
                        uriBuilder.Port = request.Host.Port.Value;

                    uriBuilder.Path = request.PathBase.Add("/OrchardCore.Users/Registration/ConfirmEmail").Value;
                    uriBuilder.Query = string.Format("userId={0}&code={1}", user.Id, UrlEncoder.Default.Encode(code));
                    workflowContext.Properties["EmailConfirmationUrl"] = uriBuilder.Uri.ToString();


                    var subject = await _expressionEvaluator.EvaluateAsync(ConfirmationEmailSubject, workflowContext);
                    var localizedSubject = new LocalizedString(nameof(RegisterUserTask), subject);

                    var body = await _expressionEvaluator.EvaluateAsync(ConfirmationEmailTemplate, workflowContext);
                    var localizedBody = new LocalizedHtmlString(nameof(RegisterUserTask), body);
                    var message = new MailMessage()
                    {
                        To = email,
                        Subject = localizedSubject.ResourceNotFound ? subject : localizedSubject.Value,
                        Body = localizedBody.IsResourceNotFound ? body : localizedBody.Value,
                        IsBodyHtml = true
                    };
                    var smtpService = _httpContextAccessor.HttpContext.RequestServices.GetService<ISmtpService>();

                    if (smtpService == null)
                    {
                        var updater = _updateModelAccessor.ModelUpdater;
                        if (updater != null)
                        {
                            updater.ModelState.TryAddModelError("", T["No email service is available"]);
                        }
                        outcome = "Invalid";
                    }
                    else
                    {
                        var result = await smtpService.SendAsync(message);
                        if (!result.Succeeded)
                        {
                            var updater = _updateModelAccessor.ModelUpdater;
                            if (updater != null)
                            {
                                foreach (var item in result.Errors)
                                {
                                    updater.ModelState.TryAddModelError(item.Name, item.Value);
                                }
                            }
                            outcome = "Invalid";
                        }
                    }
                }
            }

            return Outcomes("Done", outcome);
        }
    }
}
