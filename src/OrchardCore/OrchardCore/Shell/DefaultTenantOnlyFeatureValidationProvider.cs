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
            var feature = _extensionManager.GetFeatures().FirstOrDefault(x => x.Id == id);

            return new ValueTask<bool>(_shellSettings.Name == ShellHelper.DefaultShellName || !feature.DefaultTenantOnly);
        }

        public ValueTask<bool> IsExtensionValidAsync(string id)
        {
            var feature = _extensionManager.GetExtensions().FirstOrDefault(x => x.Id == id);

            return new ValueTask<bool>(_shellSettings.Name == ShellHelper.DefaultShellName || feature.Features.FirstOrDefault(x => x.Id == x.Id)?.DefaultTenantOnly == false);
        }
    }
}
