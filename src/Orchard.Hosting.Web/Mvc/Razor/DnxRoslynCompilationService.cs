using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Extensions.CompilationAbstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Orchard.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using DiagnosticMessage = Microsoft.AspNetCore.Diagnostics.DiagnosticMessage;

namespace Orchard.Hosting.Mvc.Razor
{
    /// <summary>
    /// A type that uses Roslyn to compile C# content and <see cref="ILibraryExporter"/> to find out references.
    /// </summary>
    public class DnxRoslynCompilationService : ICompilationService
    {
        private readonly ConcurrentDictionary<string, AssemblyMetadata> _metadataFileCache =
            new ConcurrentDictionary<string, AssemblyMetadata>(StringComparer.OrdinalIgnoreCase);

        private readonly IHostingEnvironment _environment;
        private readonly IOrchardLibraryManager _libraryManager;
        private readonly ILibraryExporter _libraryExporter;
        private readonly RazorViewEngineOptions _options;
        private readonly Lazy<List<MetadataReference>> _applicationReferences;
        private readonly IAssemblyLoadContext _loader;

        /// <summary>
        /// Initalizes a new instance of the <see cref="DnxRoslynCompilationService"/> class.
        /// </summary>
        /// <param name="environment">The environment for the executing application.</param>
        /// <param name="libraryManager">The <see cref="IOrchardLibraryManager"/>.</param>
        /// <param name="libraryExporter">The library manager that provides export and reference information.</param>
        /// <param name="optionsAccessor">Accessor to <see cref="RazorViewEngineOptions"/>.</param>
        /// <param name="fileProviderAccessor">The <see cref="IRazorViewEngineFileProviderAccessor"/>.</param>
        /// <param name="loaderAccessor"> The accessor for the <see cref="IAssemblyLoadContext"/> used to load compiled assemblies.</param>
        public DnxRoslynCompilationService(
            IHostingEnvironment environment,
            IOrchardLibraryManager libraryManager,
            ILibraryExporter libraryExporter,
            IOptions<RazorViewEngineOptions> optionsAccessor,
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            IAssemblyLoadContextAccessor loaderAccessor)
        {
            _environment = environment;
            _libraryManager = libraryManager;
            _libraryExporter = libraryExporter;
            _options = optionsAccessor.Value;
            _applicationReferences = new Lazy<List<MetadataReference>>(GetApplicationReferences);
            _loader = loaderAccessor.GetLoadContext(typeof(DnxRoslynCompilationService).GetTypeInfo().Assembly);
        }

        /// <inheritdoc />
        public CompilationResult Compile(RelativeFileInfo fileInfo, string compilationContent)
        {
            var assemblyName = Path.GetRandomFileName();

            var sourceText = SourceText.From(compilationContent, Encoding.UTF8);
            var syntaxTree = CSharpSyntaxTree.ParseText(
                sourceText,
                path: assemblyName,
                options: _options.ParseOptions);

            var references = _applicationReferences.Value;

            var compilation = CSharpCompilation.Create(
                assemblyName,
                options: _options.CompilationOptions,
                syntaxTrees: new[] { syntaxTree },
                references: references);

            using (var ms = new MemoryStream())
            {
                using (var pdb = new MemoryStream())
                {
                    var result = compilation.Emit(ms);
                    if (!result.Success)
                    {
                        return GetCompilationFailedResult(
                            fileInfo.RelativePath,
                            compilationContent,
                            assemblyName,
                            result.Diagnostics);
                    }

                    ms.Seek(0, SeekOrigin.Begin);

                    var assembly = _loader.LoadStream(ms, assemblySymbols: null);

                    var type = assembly
                        .GetExportedTypes()[0];

                    return new CompilationResult(type);
                }
            }
        }

        internal CompilationResult GetCompilationFailedResult(
            string relativePath,
            string compilationContent,
            string assemblyName,
            IEnumerable<Diagnostic> diagnostics)
        {
            var diagnosticGroups = diagnostics
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .GroupBy(diagnostic => diagnostic.Location.GetMappedLineSpan().Path, StringComparer.Ordinal);

            var failures = new List<Microsoft.AspNetCore.Diagnostics.CompilationFailure>();
            foreach (var group in diagnosticGroups)
            {
                var sourceFilePath = group.Key;

                var compilationFailure = new Microsoft.AspNetCore.Diagnostics.CompilationFailure(
                    sourceFilePath,
                    string.Empty,
                    compilationContent,
                    group.Select(GetDiagnosticMessage));

                failures.Add(compilationFailure);
            }

            return new CompilationResult(failures);
        }

        private static DiagnosticMessage GetDiagnosticMessage(Diagnostic diagnostic)
        {
            var mappedLineSpan = diagnostic.Location.GetMappedLineSpan();
            return new DiagnosticMessage(
                diagnostic.GetMessage(),
                CSharpDiagnosticFormatter.Instance.Format(diagnostic),
                mappedLineSpan.Path,
                mappedLineSpan.StartLinePosition.Line + 1,
                mappedLineSpan.StartLinePosition.Character + 1,
                mappedLineSpan.EndLinePosition.Line + 1,
                mappedLineSpan.EndLinePosition.Character + 1);
        }

        private List<MetadataReference> GetApplicationReferences()
        {
            var references = new List<MetadataReference>();

            var export = _libraryExporter.GetAllExports(_environment.ApplicationName);
            foreach (var metadataReference in _libraryManager.GetAllMetadataReferences())
            {
                if (export.MetadataReferences.All(x => x.Name != metadataReference.Name))
                    references.Add(ConvertMetadataReference(metadataReference));
            }

            // Get the MetadataReference for the executing application. If it's a Roslyn reference,
            // we can copy the references created when compiling the application to the Razor page being compiled.
            // This avoids performing expensive calls to MetadataReference.CreateFromImage.
            var libraryExport = _libraryExporter.GetExport(_environment.ApplicationName);
            if (libraryExport?.MetadataReferences != null && libraryExport.MetadataReferences.Count > 0)
            {
                Debug.Assert(libraryExport.MetadataReferences.Count == 1,
                    "Expected 1 MetadataReferences, found " + libraryExport.MetadataReferences.Count);
                var roslynReference = libraryExport.MetadataReferences[0] as IRoslynMetadataReference;
                var compilationReference = roslynReference?.MetadataReference as CompilationReference;
                if (compilationReference != null)
                {
                    references.AddRange(compilationReference.Compilation.References);
                    references.Add(roslynReference.MetadataReference);
                    return references;
                }
            }

            if (export != null)
            {
                foreach (var metadataReference in export.MetadataReferences)
                {
                    // Taken from https://github.com/aspnet/KRuntime/blob/757ba9bfdf80bd6277e715d6375969a7f44370ee/src/...
                    // Microsoft.Extensions.Runtime.Roslyn/RoslynCompiler.cs#L164
                    // We don't want to take a dependency on the Roslyn bit directly since it pulls in more dependencies
                    // than the view engine needs (Microsoft.Extensions.Runtime) for example
                    references.Add(ConvertMetadataReference(metadataReference));
                }
            }

            return references;
        }

        private MetadataReference ConvertMetadataReference(IMetadataReference metadataReference)
        {
            var roslynReference = metadataReference as IRoslynMetadataReference;

            if (roslynReference != null)
            {
                return roslynReference.MetadataReference;
            }

            var embeddedReference = metadataReference as IMetadataEmbeddedReference;

            if (embeddedReference != null)
            {
                return MetadataReference.CreateFromImage(embeddedReference.Contents);
            }

            var fileMetadataReference = metadataReference as IMetadataFileReference;

            if (fileMetadataReference != null)
            {
                return CreateMetadataFileReference(fileMetadataReference.Path);
            }

            var projectReference = metadataReference as IMetadataProjectReference;
            if (projectReference != null)
            {
                using (var ms = new MemoryStream())
                {
                    projectReference.EmitReferenceAssembly(ms);

                    return MetadataReference.CreateFromImage(ms.ToArray());
                }
            }

            throw new NotSupportedException();
        }

        private MetadataReference CreateMetadataFileReference(string path)
        {
            var metadata = _metadataFileCache.GetOrAdd(path, _ =>
            {
                using (var stream = File.OpenRead(path))
                {
                    var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
                    return AssemblyMetadata.Create(moduleMetadata);
                }
            });

            return metadata.GetReference(filePath: path);
        }
    }
}