// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Framework.Runtime.Roslyn.Services;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Framework.Runtime.Roslyn {
    public class InternalRoslynCompiler {
        private readonly ICache _cache;
        private readonly ICacheContextAccessor _cacheContextAccessor;
        private readonly INamedCacheDependencyProvider _namedDependencyProvider;
        private readonly IAssemblyLoadContextFactory _loadContextFactory;
        private readonly IFileWatcher _watcher;
        private readonly IServiceProvider _services;
        private readonly ISourceTextService _sourceTextService;

        public InternalRoslynCompiler(ICache cache,
                              ICacheContextAccessor cacheContextAccessor,
                              INamedCacheDependencyProvider namedDependencyProvider,
                              IAssemblyLoadContextFactory loadContextFactory,
                              IFileWatcher watcher,
                              IServiceProvider services) {
            _cache = cache;
            _cacheContextAccessor = cacheContextAccessor;
            _namedDependencyProvider = namedDependencyProvider;
            _loadContextFactory = loadContextFactory;
            _watcher = watcher;
            _services = services;
            _sourceTextService = (ISourceTextService)services.GetService(typeof(ISourceTextService));
        }

        public CompilationContext CompileProject(
            Project project,
            ILibraryKey target,
            IEnumerable<IMetadataReference> incomingReferences,
            IEnumerable<ISourceReference> incomingSourceReferences,
            IList<IMetadataReference> outgoingReferences) {
            var path = project.ProjectDirectory;
            var name = project.Name;
            
            var isMainAspect = string.IsNullOrEmpty(target.Aspect);
            var isPreprocessAspect = string.Equals(target.Aspect, "preprocess", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(target.Aspect)) {
                name += "!" + target.Aspect;
            }

            _watcher.WatchProject(path);

            _watcher.WatchFile(project.ProjectFilePath);

            if (_cacheContextAccessor.Current != null) {
                _cacheContextAccessor.Current.Monitor(new FileWriteTimeCacheDependency(project.ProjectFilePath));

                // Monitor the trigger {projectName}_BuildOutputs
                var buildOutputsName = project.Name + "_BuildOutputs";

                _cacheContextAccessor.Current.Monitor(_namedDependencyProvider.GetNamedDependency(buildOutputsName));
            }

            var exportedReferences = incomingReferences.Select(ConvertMetadataReference);

            Trace.TraceInformation("[{0}]: Compiling '{1}'", GetType().Name, name);
            var sw = Stopwatch.StartNew();

            var compilationSettings = project.GetCompilerOptions(target.TargetFramework, target.Configuration)
                                             .ToCompilationSettings(target.TargetFramework);

            var sourceFiles = Enumerable.Empty<String>();
            if (isMainAspect) {
                sourceFiles = project.SourceFiles;
            }
            else if (isPreprocessAspect) {
                sourceFiles = project.PreprocessSourceFiles;
            }

            var parseOptions = new CSharpParseOptions(languageVersion: compilationSettings.LanguageVersion,
                                                      preprocessorSymbols: compilationSettings.Defines.AsImmutable());

            IList<SyntaxTree> trees = GetSyntaxTrees(
                project,
                sourceFiles,
                incomingSourceReferences,
                parseOptions);

            var embeddedReferences = incomingReferences.OfType<IMetadataEmbeddedReference>()
                                                       .ToDictionary(a => a.Name, ConvertMetadataReference);

            var references = new List<MetadataReference>();
            references.AddRange(exportedReferences);

            var compilation = CSharpCompilation.Create(
                name,
                trees,
                references,
                compilationSettings.CompilationOptions);

            var aniSw = Stopwatch.StartNew();
            Trace.TraceInformation("[{0}]: Scanning '{1}' for assembly neutral interfaces", GetType().Name, name);

            var assemblyNeutralWorker = new AssemblyNeutralWorker(compilation, embeddedReferences);
            assemblyNeutralWorker.FindTypeCompilations(compilation.Assembly.GlobalNamespace);

            assemblyNeutralWorker.OrderTypeCompilations();
            var assemblyNeutralTypeDiagnostics = assemblyNeutralWorker.GenerateTypeCompilations();

            assemblyNeutralWorker.Generate();

            aniSw.Stop();
            Trace.TraceInformation("[{0}]: Found {1} assembly neutral interfaces for '{2}' in {3}ms", GetType().Name, assemblyNeutralWorker.TypeCompilations.Count(), name, aniSw.ElapsedMilliseconds);

            foreach (var t in assemblyNeutralWorker.TypeCompilations) {
                outgoingReferences.Add(new EmbeddedMetadataReference(t));
            }

            var newCompilation = assemblyNeutralWorker.Compilation;

            newCompilation = ApplyVersionInfo(newCompilation, project, parseOptions);

            var compilationContext = new CompilationContext(newCompilation,
                incomingReferences.Concat(outgoingReferences).ToList(),
                assemblyNeutralTypeDiagnostics,
                project);

            var modules = new List<ICompileModule>();

            using (var childContext = _loadContextFactory.Create()) {

                if (isMainAspect && project.PreprocessSourceFiles.Any()) {
                    try {
                        var preprocessAssembly = childContext.Load(project.Name + "!preprocess");
                        foreach (var preprocessType in preprocessAssembly.ExportedTypes) {
                            if (preprocessType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ICompileModule))) {
                                var module = (ICompileModule)ActivatorUtilities.CreateInstance(_services, preprocessType);
                                modules.Add(module);
                            }
                        }

                    }
                    catch (Exception ex) {
                        var compilationException = ex.InnerException as RoslynCompilationException;

                        if (compilationException != null) {
                            // Add diagnostics from the precompile step
                            foreach (var diag in compilationException.Diagnostics) {
                                compilationContext.Diagnostics.Add(diag);
                            }

                            Trace.TraceError("[{0}]: Failed loading meta assembly '{1}'", GetType().Name, name);
                        }
                        else {
                            Trace.TraceError("[{0}]: Failed loading meta assembly '{1}':\n {2}", GetType().Name, name, ex);
                        }
                    }
                }

                foreach (var module in modules) {
                    module.BeforeCompile(compilationContext);
                }
            }

            sw.Stop();
            Trace.TraceInformation("[{0}]: Compiled '{1}' in {2}ms", GetType().Name, name, sw.ElapsedMilliseconds);

            return compilationContext;
        }

        private static CSharpCompilation ApplyVersionInfo(CSharpCompilation compilation, Project project,
            CSharpParseOptions parseOptions) {
            var emptyVersion = new Version(0, 0, 0, 0);

            // If the assembly version is empty then set the version
            if (compilation.Assembly.Identity.Version == emptyVersion) {
                return compilation.AddSyntaxTrees(new[]
                {
                    CSharpSyntaxTree.ParseText("[assembly: System.Reflection.AssemblyVersion(\"" + project.Version.Version + "\")]", parseOptions),
                    CSharpSyntaxTree.ParseText("[assembly: System.Reflection.AssemblyInformationalVersion(\"" + project.Version + "\")]", parseOptions)
                });
            }

            return compilation;
        }

        private IList<SyntaxTree> GetSyntaxTrees(Project project,
                                                 IEnumerable<string> sourceFiles,
                                                 IEnumerable<ISourceReference> sourceReferences,
                                                 CSharpParseOptions parseOptions) {
            var trees = new List<SyntaxTree>();

            // Enumerate all sub dirs and start from that set of folders incase new folders are added
            var dirs = new HashSet<string>();
            dirs.Add(project.ProjectDirectory);

            foreach (var sourcePath in sourceFiles) {
                _watcher.WatchFile(sourcePath);

                var syntaxTree = CreateSyntaxTree(sourcePath, parseOptions);

                trees.Add(syntaxTree);
            }

            foreach (var sourceFileReference in sourceReferences.OfType<ISourceFileReference>()) {
                var sourcePath = sourceFileReference.Path;

                _watcher.WatchFile(sourcePath);

                var syntaxTree = CreateSyntaxTree(sourcePath, parseOptions);

                trees.Add(syntaxTree);
            }

            // Watch all directories
            var ctx = _cacheContextAccessor.Current;

            foreach (var d in dirs) {
                if (ctx != null) {
                    ctx.Monitor(new FileWriteTimeCacheDependency(d));
                }

                // TODO: Make the file watcher hand out cache dependencies as well
                _watcher.WatchDirectory(d, ".cs");
            }

            return trees;
        }

        private SyntaxTree CreateSyntaxTree(string sourcePath, CSharpParseOptions parseOptions) {
            // The cache key needs to take the parseOptions into account
            var cacheKey = sourcePath + string.Join(",", parseOptions.PreprocessorSymbolNames) + parseOptions.LanguageVersion;

            return _cache.Get<SyntaxTree>(cacheKey, (ctx, oldValue) => {
                SourceText sourceText;
                if (_sourceTextService != null) {
                    sourceText = _sourceTextService.GetSourceText(sourcePath);
                }
                else {
                    ctx.Monitor(new FileWriteTimeCacheDependency(sourcePath));
                    using (var stream = File.OpenRead(sourcePath)) {
                        sourceText = SourceText.From(stream, encoding: Encoding.UTF8);
                    }
                }

                if (oldValue != null && _sourceTextService != null) {
                    return oldValue.WithChangedText(sourceText);
                }
                else {
                    return CSharpSyntaxTree.ParseText(sourceText, options: parseOptions, path: sourcePath);
                }
            });
        }

        private MetadataReference ConvertMetadataReference(IMetadataReference metadataReference) {
            var roslynReference = metadataReference as IRoslynMetadataReference;

            if (roslynReference != null) {
                return roslynReference.MetadataReference;
            }

            var embeddedReference = metadataReference as IMetadataEmbeddedReference;

            if (embeddedReference != null) {
                return MetadataReference.CreateFromImage(embeddedReference.Contents);
            }

            var fileMetadataReference = metadataReference as IMetadataFileReference;

            if (fileMetadataReference != null) {
                return GetMetadataReference(fileMetadataReference.Path);
            }

            var projectReference = metadataReference as IMetadataProjectReference;
            if (projectReference != null) {
                using (var ms = new MemoryStream()) {
                    projectReference.EmitReferenceAssembly(ms);

                    return MetadataReference.CreateFromImage(ms.ToArray());
                }
            }

            throw new NotSupportedException();
        }

        private MetadataReference GetMetadataReference(string path) {
            var metadata = _cache.Get<AssemblyMetadata>(path, ctx => {
                ctx.Monitor(new FileWriteTimeCacheDependency(path));

                using (var stream = File.OpenRead(path)) {
                    var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
                    return AssemblyMetadata.Create(moduleMetadata);
                }
            });

            return metadata.GetReference();
        }
    }
}
