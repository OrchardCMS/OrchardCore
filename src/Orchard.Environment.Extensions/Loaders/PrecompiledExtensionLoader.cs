using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Orchard.DependencyInjection;
using Orchard.Environment.Extensions.Models;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions.Folders;
using Microsoft.Extensions.Options;
using Orchard.FileSystem;
using System.IO;
using Microsoft.DotNet.ProjectModel.Loader;
using Microsoft.DotNet.ProjectModel;

namespace Orchard.Environment.Extensions.Loaders
{
    public class PrecompiledExtensionLoader : IExtensionLoader
    {
        private readonly string[] ExtensionsSearchPaths;

        private readonly IHostEnvironment _hostEnvironment;
        private readonly IOrchardFileSystem _fileSystem;
        private readonly ILogger _logger;

        public PrecompiledExtensionLoader(
            IOptions<ExtensionHarvestingOptions> optionsAccessor,
            IHostEnvironment hostEnvironment,
            IOrchardFileSystem fileSystem,
            ILogger<PrecompiledExtensionLoader> logger)
        {
            ExtensionsSearchPaths = optionsAccessor.Value.ModuleLocationExpanders.SelectMany(x => x.SearchPaths).ToArray();
            _hostEnvironment = hostEnvironment;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public string Name => GetType().Name;

        public int Order => 30;

        public void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension)
        {
        }

        public void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension)
        {
        }

        public bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references)
        {
            return true;
        }

        public ExtensionEntry Load(ExtensionDescriptor descriptor)
        {
            if (!ExtensionsSearchPaths.Contains(descriptor.Location))
            {
                return null;
            }

            var extensionPath = Path.Combine(descriptor.Location, descriptor.Id);

            var projectContext = ProjectContext.CreateContextForEachFramework(extensionPath).FirstOrDefault();

            if (projectContext == null)
            {
                return null;
            }

            var loadContext = projectContext.CreateLoadContext();

            var assembly = loadContext.LoadFromAssemblyName(new AssemblyName(descriptor.Id));

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Loaded referenced extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);
            }

            return new ExtensionEntry
            {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.ExportedTypes
            };
        }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor)
        {
            return null;
        }

        public void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry)
        {
        }

        public void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry)
        {
        }
    }
}