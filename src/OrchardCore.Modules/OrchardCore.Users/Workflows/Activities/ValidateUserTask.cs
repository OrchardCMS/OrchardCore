using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Users.Workflows.Activities;

public class ValidateUserTask : TaskActivity<ValidateUserTask>, IActivityProvidedValues, IActivityDescription
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _roleClaimType;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public ValidateUserTask(IHttpContextAccessor httpContextAccessor, IOptions<IdentityOptions> optionsAccessor, IStringLocalizerFactory localizerFactory, IStringLocalizer<ValidateUserTask> localizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _roleClaimType = optionsAccessor.Value.ClaimsIdentity.RoleClaimType;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ValidateUserTask>("Validate User Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.User;

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

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

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Anonymous"], S["Authenticated"], S["InRole"]);

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

                        return Outcome("InRole");
                    }
                }
            }

            return Outcome(OrchardCoreConstants.Roles.Authenticated);
        }

        return Outcome(OrchardCoreConstants.Roles.Anonymous);
    }

    private static readonly ActivityProvidedValue s_lastResult = ActivityProvidedValue.LastResult("array", LocalizationSource.Create<ValidateUserTask>("The names of the user's roles, when the user is in one of the roles."));

    private static readonly ActivityProvidedValue s_userName = new ActivityProvidedValue { Source = WorkflowValueSource.Properties, Name = "UserName", TypeName = "string", Description = LocalizationSource.Create<ValidateUserTask>("The name of the signed-in user.") };

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        => SetUserName
            ? [s_userName, s_lastResult]
            : [s_lastResult];
}
