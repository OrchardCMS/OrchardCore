using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public class ModuleProjectLibraryExportProvider : ILibraryExportProvider {
        private readonly IProjectResolver _projectResolver;
        private readonly IServiceProvider _serviceProvider;

        public ModuleProjectLibraryExportProvider(IProjectResolver projectResolver,
            IServiceProvider serviceProvider) {
            _projectResolver = projectResolver;
            _serviceProvider = serviceProvider;
        }

        public LibraryExport GetLibraryExport(CompilationTarget target) {
            Project project;
            // Can't find a project file with the name so bail
            if (!_projectResolver.TryResolveProject(target.Name, out project)) {
                return null;
            }

            var targetFrameworkInformation = project.GetTargetFramework(target.TargetFramework);

            // This is the target framework defined in the project. If there were no target frameworks
            // defined then this is the targetFramework specified
            if (targetFrameworkInformation.FrameworkName != null) {
                target = target.ChangeTargetFramework(targetFrameworkInformation.FrameworkName);
            }

            var metadataReferences = new List<IMetadataReference>();
            var sourceReferences = new List<ISourceReference>();

            if (!string.IsNullOrEmpty(targetFrameworkInformation.AssemblyPath)) {
                var assemblyPath = ResolvePath(project, target.Configuration, targetFrameworkInformation.AssemblyPath);
                var pdbPath = ResolvePath(project, target.Configuration, targetFrameworkInformation.PdbPath);
                
                metadataReferences.Add(new CompiledProjectMetadataReference(project.ToCompilationContext(target), assemblyPath, pdbPath));
            }
            else {
                var libraryManager = _serviceProvider.GetService<IOrchardLibraryManager>();

               return libraryManager.GetLibraryExport(target.Name, target.Aspect);
            }

            return new LibraryExport(metadataReferences, sourceReferences);
        }

        private static string ResolvePath(Project project, string configuration, string path) {
            if (string.IsNullOrEmpty(path)) {
                return null;
            }

            if (Path.DirectorySeparatorChar == '/') {
                path = path.Replace('\\', Path.DirectorySeparatorChar);
            }
            else {
                path = path.Replace('/', Path.DirectorySeparatorChar);
            }

            path = path.Replace("{configuration}", configuration);

            return Path.Combine(project.ProjectDirectory, path);
        }


    }
}