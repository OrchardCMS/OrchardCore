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
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime.Common.DependencyInjection;
using OrchardVNext;

namespace Microsoft.Framework.Runtime.Roslyn {
    public class InternalRoslynCompiler {
        private readonly ICache _cache;
        private readonly ICacheContextAccessor _cacheContextAccessor;
        private readonly INamedCacheDependencyProvider _namedDependencyProvider;
        private readonly IAssemblyLoadContextFactory _loadContextFactory;
        private readonly IFileWatcher _watcher;
        private readonly IApplicationEnvironment _environment;
        private readonly IServiceProvider _services;

        public InternalRoslynCompiler(ICache cache,
                              ICacheContextAccessor cacheContextAccessor,
                              INamedCacheDependencyProvider namedDependencyProvider,
                              IAssemblyLoadContextFactory loadContextFactory,
                              IFileWatcher watcher,
                              IApplicationEnvironment environment,
                              IServiceProvider services) {
            _cache = cache;
            _cacheContextAccessor = cacheContextAccessor;
            _namedDependencyProvider = namedDependencyProvider;
            _loadContextFactory = loadContextFactory;
            _watcher = watcher;
            _environment = environment;
            _services = services;
        }

        public CompilationContext CompileProject(
            Project project,
            ILibraryKey target,
            IEnumerable<IMetadataReference> incomingReferences,
            IEnumerable<ISourceReference> incomingSourceReferences) {
            var path = project.ProjectDirectory;
            var name = project.Name.TrimStart('/');

            var isMainAspect = string.IsNullOrEmpty(target.Aspect);
            var isPreprocessAspect = string.Equals(target.Aspect, "preprocess", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(target.Aspect)) {
                name += "!" + target.Aspect;
            }

            _watcher.WatchProject(path);

            _watcher.WatchFile(project.ProjectFilePath);

            if (_cacheContextAccessor.Current != null) {
                _cacheContextAccessor.Current.Monitor(new FileWriteTimeCacheDependency(project.ProjectFilePath));

                if (isMainAspect) {
                    // Monitor the trigger {projectName}_BuildOutputs
                    var buildOutputsName = project.Name + "_BuildOutputs";

                    _cacheContextAccessor.Current.Monitor(_namedDependencyProvider.GetNamedDependency(buildOutputsName));
                }
            }

            var exportedReferences = incomingReferences.Select(ConvertMetadataReference);

            Logger.TraceInformation("[{0}]: Compiling '{1}'", GetType().Name, name);
            var sw = Stopwatch.StartNew();

            var compilationSettings = project.GetCompilerOptions(target.TargetFramework, target.Configuration)
                                             .ToCompilationSettings(target.TargetFramework);

            var sourceFiles = Enumerable.Empty<String>();
            if (isMainAspect) {
                sourceFiles = project.Files.SourceFiles;
            }
            else if (isPreprocessAspect) {
                sourceFiles = project.Files.PreprocessSourceFiles;
            }

            var parseOptions = new CSharpParseOptions(languageVersion: compilationSettings.LanguageVersion,
                                                      preprocessorSymbols: compilationSettings.Defines);

            IList<SyntaxTree> trees = GetSyntaxTrees(
                project,
                sourceFiles,
                incomingSourceReferences,
                parseOptions,
                isMainAspect);

            var embeddedReferences = incomingReferences.OfType<IMetadataEmbeddedReference>()
                                                       .ToDictionary(a => a.Name, ConvertMetadataReference);

            var references = new List<MetadataReference>();
            references.AddRange(exportedReferences);

            var compilation = CSharpCompilation.Create(
                name,
                trees,
                references,
                compilationSettings.CompilationOptions);

            compilation = ApplyVersionInfo(compilation, project, parseOptions);

            var compilationContext = new CompilationContext(compilation, project, target.TargetFramework, target.Configuration);

            if (isMainAspect && project.Files.PreprocessSourceFiles.Any()) {
                try {
                    var modules = GetCompileModules(target).Modules;

                    foreach (var m in modules) {
                        compilationContext.Modules.Add(m);
                    }
                }
                catch (Exception ex) {
                    var compilationException = ex.InnerException as RoslynCompilationException;

                    if (compilationException != null) {
                        // Add diagnostics from the precompile step
                        foreach (var diag in compilationException.Diagnostics) {
                            compilationContext.Diagnostics.Add(diag);
                        }

                        Logger.TraceError("[{0}]: Failed loading meta assembly '{1}'", GetType().Name, name);
                    }
                    else {
                        Logger.TraceError("[{0}]: Failed loading meta assembly '{1}':\n {2}", GetType().Name, name, ex);
                    }
                }
            }

            if (compilationContext.Modules.Count > 0) {
                var precompSw = Stopwatch.StartNew();
                foreach (var module in compilationContext.Modules) {
                    module.BeforeCompile(compilationContext);
                }

                precompSw.Stop();
                Logger.TraceInformation("[{0}]: Compile modules ran in in {1}ms", GetType().Name, precompSw.ElapsedMilliseconds);
            }

            sw.Stop();
            Logger.TraceInformation("[{0}]: Compiled '{1}' in {2}ms", GetType().Name, name, sw.ElapsedMilliseconds);

            return compilationContext;
        }

        private CompilationModules GetCompileModules(ILibraryKey target) {
            // The only thing that matters is the runtime environment
            // when loading the compilation modules, so use that as the cache key
            var key = Tuple.Create(
                target.Name,
                _environment.RuntimeFramework,
                _environment.Configuration,
                "compilemodules");

            return _cache.Get<CompilationModules>(key, _ => {
                var modules = new List<ICompileModule>();

                var childContext = _loadContextFactory.Create();

                var preprocessAssembly = childContext.Load(target.Name + "!preprocess");

                foreach (var preprocessType in preprocessAssembly.ExportedTypes) {
                    if (preprocessType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ICompileModule))) {
                        var module = (ICompileModule)ActivatorUtilities.CreateInstance(_services, preprocessType);
                        modules.Add(module);
                    }
                }

                // We do this so that the load context is disposed when the cache entry
                // expires
                return new CompilationModules {
                    LoadContext = childContext,
                    Modules = modules,
                };
            });
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
                                                 CSharpParseOptions parseOptions,
                                                 bool isMainAspect) {
            var trees = new List<SyntaxTree>();

            var dirs = new HashSet<string>();

            if (isMainAspect) {
                dirs.Add(project.ProjectDirectory);
            }

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
                if (ctx != null)
                    ctx.Monitor(new FileWriteTimeCacheDependency(d));

                // TODO: Make the file watcher hand out cache dependencies as well
                _watcher.WatchDirectory(d, ".cs");
            }

            return trees;
        }

        private SyntaxTree CreateSyntaxTree(string sourcePath, CSharpParseOptions parseOptions) {
            // The cache key needs to take the parseOptions into account
            var cacheKey = sourcePath + string.Join(",", parseOptions.PreprocessorSymbolNames) + parseOptions.LanguageVersion;

            return _cache.Get<SyntaxTree>(cacheKey, ctx => {
                ctx.Monitor(new FileWriteTimeCacheDependency(sourcePath));
                using (var stream = File.OpenRead(sourcePath)) {
                    var sourceText = SourceText.From(stream, encoding: Encoding.UTF8);

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

        private class CompilationModules : IDisposable {
            public IAssemblyLoadContext LoadContext { get; set; }
            public List<ICompileModule> Modules { get; set; }

            public void Dispose() {
                LoadContext.Dispose();
            }
        }
    }
}
