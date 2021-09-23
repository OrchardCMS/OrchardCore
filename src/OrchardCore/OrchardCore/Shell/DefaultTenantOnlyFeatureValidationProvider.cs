using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Environment.Shell
{
    public class DefaultTenantOnlyFeatureValidationProvider : IFeatureValidationProvider
    {
        private readonly IExtensionManager _extensionManager;
        private readonly ShellSettings _shellSettings;

        public DefaultTenantOnlyFeatureValidationProvider(
            IExtensionManager extensionManager,
            ShellSettings shellSettings
            )
        {
            _extensionManager = extensionManager;
            _shellSettings = shellSettings;
        }

        public ValueTask<bool> IsFeatureValidAsync(string id)
        {
            if (_shellSettings.Name == ShellHelper.DefaultShellName)
            {
                return new ValueTask<bool>(true);
            }

            var features = _extensionManager.GetFeatures(new[] { id });
            if (!features.Any())
            {
                return new ValueTask<bool>(false);
            }

            return new ValueTask<bool>(!features.Any(f => f.DefaultTenantOnly));
        }
    }
}
