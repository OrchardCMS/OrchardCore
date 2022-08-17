using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Setup;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using OrchardCore.Setup.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities
{
    public class SetupTenantTask : TenantTask
    {
        private readonly IClock _clock;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IEmailAddressValidator _emailAddressValidator;
        private readonly IdentityOptions _identityOptions;

        public SetupTenantTask(
            IClock clock,
            IUpdateModelAccessor updateModelAccessor,
            IEmailAddressValidator emailAddressValidator,
            IOptions<IdentityOptions> identityOptions,
            IShellSettingsManager shellSettingsManager,
            IShellHost shellHost,
            ISetupService setupService,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IWorkflowScriptEvaluator scriptEvaluator,
            IStringLocalizer<SetupTenantTask> localizer)
            : base(shellSettingsManager, shellHost, expressionEvaluator, scriptEvaluator, localizer)
        {
            SetupService = setupService;
            _clock = clock;
            _updateModelAccessor = updateModelAccessor;
            _emailAddressValidator = emailAddressValidator;
            _identityOptions = identityOptions.Value;
        }

        protected ISetupService SetupService { get; }

        public override string Name => nameof(SetupTenantTask);

        public override LocalizedString Category => S["Tenant"];

        public override LocalizedString DisplayText => S["Setup Tenant Task"];

        public WorkflowExpression<string> SiteName
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> AdminUsername
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> AdminEmail
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> AdminPassword
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> DatabaseProvider
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> DatabaseConnectionString
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> DatabaseTablePrefix
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> RecipeName
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            if (!ShellScope.Context.Settings.IsDefaultShell())
            {
                return Outcomes("Failed");
            }

            var tenantName = (await ExpressionEvaluator.EvaluateAsync(TenantName, workflowContext, null))?.Trim();

            if (string.IsNullOrWhiteSpace(tenantName))
            {
                return Outcomes("Failed");
            }

            if (!ShellHost.TryGetSettings(tenantName, out var shellSettings))
            {
                return Outcomes("Failed");
            }

            if (shellSettings.State == TenantState.Running)
            {
                return Outcomes("Failed");
            }

            if (shellSettings.State != TenantState.Uninitialized)
            {
                return Outcomes("Failed");
            }

            var siteName = (await ExpressionEvaluator.EvaluateAsync(SiteName, workflowContext, null))?.Trim();
            var adminUsername = (await ExpressionEvaluator.EvaluateAsync(AdminUsername, workflowContext, null))?.Trim();
            var adminEmail = (await ExpressionEvaluator.EvaluateAsync(AdminEmail, workflowContext, null))?.Trim();

            if (string.IsNullOrEmpty(adminUsername) || adminUsername.Any(c => !_identityOptions.User.AllowedUserNameCharacters.Contains(c)))
            {
                return Outcomes("Failed");
            }

            if (string.IsNullOrEmpty(adminEmail) || !_emailAddressValidator.Validate(adminEmail))
            {
                return Outcomes("Failed");
            }

            var adminPassword = (await ExpressionEvaluator.EvaluateAsync(AdminPassword, workflowContext, null))?.Trim();

            var databaseProvider = (await ExpressionEvaluator.EvaluateAsync(DatabaseProvider, workflowContext, null))?.Trim();
            var databaseConnectionString = (await ExpressionEvaluator.EvaluateAsync(DatabaseConnectionString, workflowContext, null))?.Trim();
            var databaseTablePrefix = (await ExpressionEvaluator.EvaluateAsync(DatabaseTablePrefix, workflowContext, null))?.Trim();
            var recipeName = (await ExpressionEvaluator.EvaluateAsync(RecipeName, workflowContext, null))?.Trim();

            if (string.IsNullOrEmpty(databaseProvider))
            {
                databaseProvider = shellSettings["DatabaseProvider"];
            }

            if (string.IsNullOrEmpty(databaseConnectionString))
            {
                databaseConnectionString = shellSettings["ConnectionString"];
            }

            if (string.IsNullOrEmpty(databaseTablePrefix))
            {
                databaseTablePrefix = shellSettings["TablePrefix"];
            }

            if (string.IsNullOrEmpty(recipeName))
            {
                recipeName = shellSettings["RecipeName"];
            }

            var recipes = await SetupService.GetSetupRecipesAsync();
            var recipe = recipes.FirstOrDefault(r => r.Name == recipeName);

            var setupContext = new SetupContext
            {
                ShellSettings = shellSettings,
                EnabledFeatures = null,
                Errors = new Dictionary<string, string>(),
                Recipe = recipe,
                Properties = new Dictionary<string, object>
                {
                    { SetupConstants.SiteName, siteName },
                    { SetupConstants.AdminUsername, adminUsername },
                    { SetupConstants.AdminEmail, AdminEmail },
                    { SetupConstants.AdminPassword, adminPassword },
                    { SetupConstants.SiteTimeZone, _clock.GetSystemTimeZone().TimeZoneId },
                    { SetupConstants.DatabaseProvider, databaseProvider },
                    { SetupConstants.DatabaseConnectionString, databaseConnectionString },
                    { SetupConstants.DatabaseTablePrefix, databaseTablePrefix },
                }
            };

            var executionId = await SetupService.SetupAsync(setupContext);

            // Check if a component in the Setup failed
            if (setupContext.Errors.Any())
            {
                var updater = _updateModelAccessor.ModelUpdater;

                foreach (var error in setupContext.Errors)
                {
                    updater.ModelState.AddModelError(error.Key, error.Value);
                }

                return Outcomes("Failed");
            }

            return Outcomes("Done");
        }
    }
}
