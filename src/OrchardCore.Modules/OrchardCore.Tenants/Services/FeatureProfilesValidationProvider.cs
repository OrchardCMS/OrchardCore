using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Tenants.Services
{
    public class FeatureProfilesValidationProvider : IFeatureValidationProvider
    {
        private readonly IExtensionManager _extensionManager;
        private readonly FeatureProfilesRuleOptions _featureProfilesRuleOptions;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        // Cached across requests as this is called a lot and can be calculated once.
        private readonly Dictionary<string, bool> _allowed = new(StringComparer.OrdinalIgnoreCase);
        private (bool NotFound, FeatureProfile FeatureProfile) _featureProfileLookup;

        public FeatureProfilesValidationProvider(
            IExtensionManager extensionManager,
            IShellHost shellHost,
            ShellSettings shellSettings,
            IOptions<FeatureProfilesRuleOptions> featureOptions)
        {
            _extensionManager = extensionManager;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _featureProfilesRuleOptions = featureOptions.Value;
        }

        public async ValueTask<bool> IsFeatureValidAsync(string id)
        {
            var featureProfile = _shellSettings["FeatureProfile"];

            if (String.IsNullOrEmpty(featureProfile))
            {
                return true;
            }

            if (!_featureProfileLookup.NotFound)
            {
                var scope = await _shellHost.GetScopeAsync(ShellHelper.DefaultShellName);
                await scope.UsingAsync(async (scope) =>
                {
                    var featureProfilesService = scope.ServiceProvider.GetService<IFeatureProfilesService>();

                    var feauterProfiles = await featureProfilesService.GetFeatureProfilesAsync();

                    foreach(var featureProfileName in featureProfile.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (feauterProfiles.TryGetValue(featureProfileName, out var featureProfile))
                        {
                            _featureProfileLookup = (false, featureProfile);

                            continue;
                        }
             
                         _featureProfileLookup = (true, null);
                    }

                });
            }

            // When the management feature is not enabled we need to pass feature validation.
            if (_featureProfileLookup.NotFound || _featureProfileLookup.FeatureProfile is null)
            {
                return true;
            }

            var isAllowed = IsAllowed(id);
            if (!isAllowed)
            {
                return false;
            }

            var dependencies = _extensionManager.GetFeatureDependencies(id);
            foreach (var dependency in dependencies)
            {
                isAllowed = IsAllowed(dependency.Id);
                if (!isAllowed)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsAllowed(string id)
        {
            if (!_allowed.TryGetValue(id, out var isAllowed))
            {
                isAllowed = true;
                foreach (var rule in _featureProfileLookup.FeatureProfile.FeatureRules)
                {
                    if (_featureProfilesRuleOptions.Rules.TryGetValue(rule.Rule, out var ruleSet))
                    {
                        // Does rule match?
                        var result = ruleSet(rule.Expression, id);
                        if (result.isMatch)
                        {
                            isAllowed = result.isAllowed;
                        }
                    }
                }
                _allowed[id] = isAllowed;
            }

            return isAllowed;
        }
    }
}
