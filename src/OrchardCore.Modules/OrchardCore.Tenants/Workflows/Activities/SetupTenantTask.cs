using System;
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
using YesSql;

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
        public WorkflowExpression<string> TableNameSeparator
        {
            get => GetProperty(() => new WorkflowExpression<string>("_"));
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Schema
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> DocumentTable
        {
            get => GetProperty(() => new WorkflowExpression<string>("Document"));
            set => SetProperty(value);
        }

        public IdentityColumnSize IdentityColumnSize
        {
            get => GetProperty(() => IdentityColumnSize.Int32);
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

            if (String.IsNullOrEmpty(adminUsername) || adminUsername.Any(c => !_identityOptions.User.AllowedUserNameCharacters.Contains(c)))
            {
                return Outcomes("Failed");
            }

            if (String.IsNullOrEmpty(adminEmail) || !_emailAddressValidator.Validate(adminEmail))
            {
                return Outcomes("Failed");
            }

            var adminPassword = (await ExpressionEvaluator.EvaluateAsync(AdminPassword, workflowContext, null))?.Trim();

            var databaseProvider = (await ExpressionEvaluator.EvaluateAsync(DatabaseProvider, workflowContext, null))?.Trim();
            var databaseConnectionString = (await ExpressionEvaluator.EvaluateAsync(DatabaseConnectionString, workflowContext, null))?.Trim();
            var databaseTablePrefix = (await ExpressionEvaluator.EvaluateAsync(DatabaseTablePrefix, workflowContext, null))?.Trim();
            var recipeName = (await ExpressionEvaluator.EvaluateAsync(RecipeName, workflowContext, null))?.Trim();
            var tableNameSeparator = (await ExpressionEvaluator.EvaluateAsync(TableNameSeparator, workflowContext, null))?.Trim();
            var schema = (await ExpressionEvaluator.EvaluateAsync(Schema, workflowContext, null))?.Trim();
            var documentTable = (await ExpressionEvaluator.EvaluateAsync(DocumentTable, workflowContext, null))?.Trim();

            if (String.IsNullOrEmpty(databaseProvider))
            {
                databaseProvider = shellSettings["DatabaseProvider"];
            }

            if (String.IsNullOrEmpty(databaseConnectionString))
            {
                databaseConnectionString = shellSettings["ConnectionString"];
            }

            if (String.IsNullOrEmpty(databaseTablePrefix))
            {
                databaseTablePrefix = shellSettings["TablePrefix"];
            }

            if (String.IsNullOrEmpty(recipeName))
            {
                recipeName = shellSettings["RecipeName"];
            }

            if (String.IsNullOrEmpty(schema))
            {
                schema = shellSettings["Schema"];
            }

            if (String.IsNullOrEmpty(documentTable))
            {
                documentTable = shellSettings["DocumentTable"];
            }

            if (String.IsNullOrEmpty(tableNameSeparator))
            {
                tableNameSeparator = shellSettings["TableNameSeparator"];
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
                    { SetupConstants.Schema, schema },
                    { SetupConstants.DocumentTable, documentTable },
                    { SetupConstants.TableNameSeparator, tableNameSeparator },
                    { SetupConstants.IdentityColumnSize, IdentityColumnSize },
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
