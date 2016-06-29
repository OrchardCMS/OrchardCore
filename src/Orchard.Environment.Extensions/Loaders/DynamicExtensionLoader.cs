using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Extensions.Folders;
using Microsoft.Extensions.Options;
using Orchard.FileSystem;
using System.Diagnostics;
using System.IO;
using System.Runtime.Loader;

namespace Orchard.Environment.Extensions.Loaders
{
    public class DynamicExtensionLoader : IExtensionLoader
    {
        private readonly string[] ExtensionsSearchPaths;

        private readonly IHostEnvironment _hostEnvironment;
        private readonly IOrchardFileSystem _fileSystem;

        public DynamicExtensionLoader(
            IOptions<ExtensionHarvestingOptions> optionsAccessor,
            IHostEnvironment hostEnvironment,
            IOrchardFileSystem fileSystem)
        {
            ExtensionsSearchPaths = optionsAccessor.Value.ExtensionLocationExpanders.SelectMany(x => x.SearchPaths).ToArray();
            _hostEnvironment = hostEnvironment;
            _fileSystem = fileSystem;
        }

        public string Name => GetType().Name;

        public int Order => 100;

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
            //var probingFolder = _fileSystem.GetDirectoryInfo("bin");
            //if (!probingFolder.Exists)
            //{
            //    probingFolder.Create();
            //}

            //var location = Path.Combine(_fileSystem.RootPath, descriptor.Location, descriptor.Id);

            //Process.Start("dotnet", $"build \"{location}\" --output \"{probingFolder}\" --framework netstandard1.5").WaitForExit();

            //var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(probingFolder.FullName, descriptor.Id) + ".dll");

            //if (assembly == null)
            //{
            //    return null;
            //}

            //return new ExtensionEntry
            //{
            //    Descriptor = descriptor,
            //    Assembly = assembly,
            //    ExportedTypes = assembly.ExportedTypes
            //};

            return null;
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