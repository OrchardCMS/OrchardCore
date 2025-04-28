using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Email;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class RegisterUserTask : TaskActivity<RegisterUserTask>
{
    private readonly IUserService _userService;
    private readonly UserManager<IUser> _userManager;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly HtmlEncoder _htmlEncoder;

    protected readonly IStringLocalizer S;

    public RegisterUserTask(
        IUserService userService,
        UserManager<IUser> userManager,
        IWorkflowExpressionEvaluator expressionEvaluator,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        IUpdateModelAccessor updateModelAccessor,
        IStringLocalizer<RegisterUserTask> localizer,
        HtmlEncoder htmlEncoder)
    {
        _userService = userService;
        _userManager = userManager;
        _expressionEvaluator = expressionEvaluator;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _updateModelAccessor = updateModelAccessor;
        S = localizer;
        _htmlEncoder = htmlEncoder;
    }

    public override LocalizedString DisplayText => S["Register User Task"];

    // The category to which this activity belongs. The activity picker groups activities by this category.
    public override LocalizedString Category => S["User"];

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
    public bool RequireModeration
    {
        get => GetProperty(() => false);
        set => SetProperty(value);
    }

    // Returns the possible outcomes of this activity.
    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Done"], S["Valid"], S["Invalid"]);
    }

    // This is the heart of the activity and actually performs the work to be done.
    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var email = GetPropertyFromFormOrContext(workflowContext, "Email");
        if (string.IsNullOrWhiteSpace(email))
        {
            return Outcomes("Done", "Invalid");
        }

        var userName = GetPropertyFromFormOrContext(workflowContext, "UserName") ?? email.Replace('@', '+');
        var user = await CreateUserAsync(userName, email);

        if (user == null)
        {
            return Outcomes("Done", "Invalid");
        }

        if (SendConfirmationEmail && !await SendConfirmationEmailAsync(user, workflowContext, email))
        {
            return Outcomes("Done", "Invalid");
        }

        return Outcomes("Done", "Valid");
    }

    private string GetPropertyFromFormOrContext(WorkflowExecutionContext context, string key)
    {
        var form = _httpContextAccessor.HttpContext?.Request.Form;
        var value = form?[key];

        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (context.Properties.TryGetValue(key, out var objValue) && objValue is string strValue)
        {
            return strValue;
        }

        return null;
    }

    private async Task<User> CreateUserAsync(string userName, string email)
    {
        var errors = new Dictionary<string, string>();
        var user = (User)await _userService.CreateUserAsync(new User
        {
            UserName = userName,
            Email = email,
            IsEnabled = !RequireModeration,
        }, null, errors.Add);

        if (errors.Count > 0)
        {
            var updater = _updateModelAccessor.ModelUpdater;
            if (updater != null)
            {
                foreach (var item in errors)
                {
                    updater.ModelState.TryAddModelError(item.Key, S[item.Value]);
                }
            }
            return null;
        }

        return user;
    }

    private async Task<bool> SendConfirmationEmailAsync(User user, WorkflowExecutionContext context, string email)
    {
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var uri = _linkGenerator.GetUriByAction(_httpContextAccessor.HttpContext, "ConfirmEmail", "Registration",
            new { area = UserConstants.Features.Users, userId = user.UserId, code });

        context.Properties["EmailConfirmationUrl"] = uri;

        var subject = await _expressionEvaluator.EvaluateAsync(ConfirmationEmailSubject, context, null);

        var body = await _expressionEvaluator.EvaluateAsync(ConfirmationEmailTemplate, context, _htmlEncoder);

        var result = await _emailService.SendAsync(email, subject, body);

        if (!result.Succeeded)
        {
            var updater = _updateModelAccessor.ModelUpdater;
            if (updater != null)
            {
                foreach (var error in result.Errors)
                {
                    foreach (var errorMessage in error.Value)
                    {
                        updater.ModelState.TryAddModelError(error.Key, errorMessage);
                    }
                }
            }

            return false;
        }

        return true;
    }
}
