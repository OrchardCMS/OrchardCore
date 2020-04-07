using System;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Setup.Events;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Handlers
{
    public class SetupEventHandler : ISetupEventHandler
    {
        private readonly IWorkflowManager _workflowManager;

        public SetupEventHandler(IWorkflowManager workflowManager)
        {
            _workflowManager = workflowManager;
        }

        public Task Setup(string siteName,
            string userName,
            string email,
            string password,
            string dbProvider,
            string dbConnectionString,
            string dbTablePrefix,
            string siteTimeZone,
            Action<string, string> reportError)
        {
            return _workflowManager.TriggerEventAsync(nameof(TenantSetupEvent),
                input: new { AdminUserName = userName, AdminEmail = email, AdminPassword = password, DatabaseProvider = dbProvider, DatabaseConnectionString = dbConnectionString, DatabaseTablePrefix = dbTablePrefix, SiteTimeZone = siteTimeZone },
                correlationId: siteName
            );
        }

    }
}
