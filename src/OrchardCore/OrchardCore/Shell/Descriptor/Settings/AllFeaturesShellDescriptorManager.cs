using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell.Descriptor.Settings
{
    /// <summary>
    /// Implements <see cref="IShellDescriptorManager"/> by returning a single tenant with all the available
    /// extensions.
    /// </summary>
    public class AllFeaturesShellDescriptorManager : IShellDescriptorManager
    {
        private readonly IExtensionManager _extensionManager;
        private ShellDescriptor _shellDescriptor;

        public AllFeaturesShellDescriptorManager(IExtensionManager extensionManager)
        {
            _extensionManager = extensionManager;
        }

        public Task<ShellDescriptor> GetShellDescriptorAsync()
        {
            _shellDescriptor ??= new ShellDescriptor
            {
                Features = _extensionManager.GetFeatures().Select(x => new ShellFeature { Id = x.Id }).ToList(),
            };

            return Task.FromResult(_shellDescriptor);
        }

        public Task UpdateShellDescriptorAsync(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures)
        {
            return Task.CompletedTask;
        }
    }
}
