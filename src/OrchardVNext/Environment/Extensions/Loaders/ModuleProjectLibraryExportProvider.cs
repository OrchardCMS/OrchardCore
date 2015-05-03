using Microsoft.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime.Compilation;
using System.Linq;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public class ModuleProjectLibraryExportProvider : ILibraryExportProvider {
        private readonly IProjectResolver _projectResolver;
        private readonly IServiceProvider _serviceProvider;

        public ModuleProjectLibraryExportProvider(IProjectResolver projectResolver,
                                            IServiceProvider serviceProvider) {
            _projectResolver = projectResolver;
            _serviceProvider = serviceProvider;
        }
        public ILibraryExport GetLibraryExport(ILibraryKey target) {
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

                metadataReferences.Add(new CompiledProjectMetadataReference(project, assemblyPath, pdbPath));
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

    internal static class LibraryKeyExtensions {
        public static ILibraryKey ChangeName(this ILibraryKey target, string name) {
            return new LibraryKey {
                Name = name,
                TargetFramework = target.TargetFramework,
                Configuration = target.Configuration,
                Aspect = target.Aspect,
            };
        }

        public static ILibraryKey ChangeTargetFramework(this ILibraryKey target, FrameworkName targetFramework) {
            return new LibraryKey {
                Name = target.Name,
                TargetFramework = targetFramework,
                Configuration = target.Configuration,
                Aspect = target.Aspect,
            };
        }

        public static ILibraryKey ChangeAspect(this ILibraryKey target, string aspect) {
            return new LibraryKey {
                Name = target.Name,
                TargetFramework = target.TargetFramework,
                Configuration = target.Configuration,
                Aspect = aspect,
            };
        }
    }

    internal class LibraryExport : ILibraryExport {
        public LibraryExport(IList<IMetadataReference> metadataReferences, IList<ISourceReference> sourceReferences) {
            MetadataReferences = metadataReferences ?? new List<IMetadataReference>();
            SourceReferences = sourceReferences ?? new List<ISourceReference>();
        }

        public IList<IMetadataReference> MetadataReferences { get; }
        public IList<ISourceReference> SourceReferences { get; }
    }

    internal class CompiledProjectMetadataReference : IMetadataProjectReference, IMetadataFileReference {
        private readonly ICompilationProject _project;
        private readonly string _assemblyPath;
        private readonly string _pdbPath;

        public CompiledProjectMetadataReference(ICompilationProject project, string assemblyPath, string pdbPath) {
            Name = project.Name;
            ProjectPath = project.ProjectFilePath;
            Path = assemblyPath;

            _project = project;
            _assemblyPath = assemblyPath;
            _pdbPath = pdbPath;
        }

        public string Name { get; private set; }

        public string ProjectPath { get; private set; }

        public string Path { get; private set; }

        public IDiagnosticResult GetDiagnostics() {
            return DiagnosticResult.Successful;
        }

        public IList<ISourceReference> GetSources() {
            return _project.Files.SourceFiles.Select(p => (ISourceReference)new SourceFileReference(p))
                                        .ToList();
        }

        public Assembly Load(IAssemblyLoadContext loadContext) {
            return loadContext.LoadFile(_assemblyPath);
        }

        public void EmitReferenceAssembly(Stream stream) {
            using (var fs = File.OpenRead(_assemblyPath)) {
                fs.CopyTo(stream);
            }
        }

        public IDiagnosticResult EmitAssembly(string outputPath) {
            Copy(_assemblyPath, outputPath);
            Copy(_pdbPath, outputPath);

            return DiagnosticResult.Successful;
        }

        private static void Copy(string sourcePath, string outputPath) {
            if (string.IsNullOrEmpty(sourcePath)) {
                return;
            }

            if (!File.Exists(sourcePath)) {
                return;
            }

            Directory.CreateDirectory(outputPath);

            File.Copy(sourcePath, System.IO.Path.Combine(outputPath, System.IO.Path.GetFileName(sourcePath)), overwrite: true);
        }
    }

    internal struct DiagnosticResult : IDiagnosticResult {
        public static readonly DiagnosticResult Successful = new DiagnosticResult(success: true,
                                                                                  diagnostics: Enumerable.Empty<ICompilationMessage>());

        public DiagnosticResult(bool success, IEnumerable<ICompilationMessage> diagnostics) {
            Success = success;
            Diagnostics = diagnostics.ToList();
        }

        public bool Success { get; }

        public IEnumerable<ICompilationMessage> Diagnostics { get; }
    }

    internal class SourceFileReference : ISourceFileReference {
        public SourceFileReference(string path) {
            // Unique name of the reference
            Name = path;
            Path = path;
        }

        public string Name { get; private set; }

        public string Path { get; private set; }
    }
}