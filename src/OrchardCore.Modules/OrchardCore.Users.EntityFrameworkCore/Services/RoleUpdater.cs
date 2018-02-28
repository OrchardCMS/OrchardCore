using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Setup.Events;

namespace OrchardCore.Users.EntityFrameworkCore.Services
{
    public class RoleUpdater: Roles.Services.RoleUpdater, ISetupEventHandler, IFeatureEventHandler
    {
        private readonly IShellFeaturesManager _featuresManager;
        private readonly ShellSettings _shellSettings;

        public RoleUpdater(RoleManager<IRole> roleManager, IEnumerable<IPermissionProvider> permissionProviders, ITypeFeatureProvider typeFeatureProvider, ILogger<RoleUpdater> logger, IShellFeaturesManager featuresManager, ShellSettings shellSettings) : base(roleManager, permissionProviders, typeFeatureProvider, logger)
        {
            _featuresManager = featuresManager;
            _shellSettings = shellSettings;
        }

        async void IFeatureEventHandler.Installed(IFeatureInfo feature)
        {
            if (_shellSettings.State == Environment.Shell.Models.TenantState.Running)
            {
                await AddDefaultRolesForFeatureAsync(feature);
            }
        }

        public async Task Setup(string siteName, string userName, string email, string password, string dbProvider,
            string dbConnectionString, string dbTablePrefix, Action<string, string> reportError)
        {
            var enabledFeatures = await _featuresManager.GetEnabledFeaturesAsync();
            foreach (var feature in enabledFeatures)
            {
                await AddDefaultRolesForFeatureAsync(feature);
            }
        }
    }
}
