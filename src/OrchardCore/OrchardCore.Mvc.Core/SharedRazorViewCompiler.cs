using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    /// <summary>
    /// Caches the result of runtime compilation of Razor files for the duration of the application lifetime.
    /// </summary>
    public class SharedRazorViewCompiler : IViewCompiler
    {
        private readonly static object _cacheLock = new object();
        private readonly Dictionary<string, Task<CompiledViewDescriptor>> _precompiledViewLookup;
        private readonly ConcurrentDictionary<string, string> _normalizedPathLookup;
        private readonly IFileProvider _fileProvider;
        private readonly RazorTemplateEngine _templateEngine;
        private readonly Action<RoslynCompilationContext> _compilationCallback;
        private readonly ILogger _logger;
        private readonly CSharpCompiler _csharpCompiler;
        private readonly static IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public SharedRazorViewCompiler(
            IFileProvider fileProvider,
            RazorTemplateEngine templateEngine,
            CSharpCompiler csharpCompiler,
            Action<RoslynCompilationContext> compilationCallback,
            IList<CompiledViewDescriptor> precompiledViews,
            ILogger logger)
        {
            if (fileProvider == null)
            {
                throw new ArgumentNullException(nameof(fileProvider));
            }

            if (templateEngine == null)
            {
                throw new ArgumentNullException(nameof(templateEngine));
            }

            if (csharpCompiler == null)
            {
                throw new ArgumentNullException(nameof(csharpCompiler));
            }

            if (compilationCallback == null)
            {
                throw new ArgumentNullException(nameof(compilationCallback));
            }

            if (precompiledViews == null)
            {
                throw new ArgumentNullException(nameof(precompiledViews));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _fileProvider = fileProvider;
            _templateEngine = templateEngine;
            _csharpCompiler = csharpCompiler;
            _compilationCallback = compilationCallback;
            _logger = logger;

            _normalizedPathLookup = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

            _precompiledViewLookup = new Dictionary<string, Task<CompiledViewDescriptor>>(
                precompiledViews.Count,
                StringComparer.OrdinalIgnoreCase);

            foreach (var precompiledView in precompiledViews)
            {
                if (_precompiledViewLookup.TryGetValue(precompiledView.RelativePath, out var otherValue))
                {
                    throw new InvalidOperationException("A precompiled view with the same name already exists: " + otherValue.Result.RelativePath);
                }

                _precompiledViewLookup.Add(precompiledView.RelativePath, Task.FromResult(precompiledView));
            }
        }

        /// <inheritdoc />
        public Task<CompiledViewDescriptor> CompileAsync(string relativePath)
        {
            if (relativePath == null)
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            // Lookup precompiled views first.

            // Attempt to lookup the cache entry using the passed in path. This will succeed if the path is already
            // normalized and a cache entry exists.
            string normalizedPath = null;
            Task<CompiledViewDescriptor> cachedResult;

            if (_precompiledViewLookup.Count > 0)
            {
                if (_precompiledViewLookup.TryGetValue(relativePath, out cachedResult))
                {
                    return cachedResult;
                }

                normalizedPath = GetNormalizedPath(relativePath);
                if (_precompiledViewLookup.TryGetValue(normalizedPath, out cachedResult))
                {
                    return cachedResult;
                }
            }

            if (_cache.TryGetValue(relativePath, out cachedResult))
            {
                return cachedResult;
            }

            normalizedPath = normalizedPath ?? GetNormalizedPath(relativePath);
            if (_cache.TryGetValue(normalizedPath, out cachedResult))
            {
                return cachedResult;
            }

            // Entry does not exist. Attempt to create one.
            cachedResult = CreateCacheEntry(normalizedPath);
            return cachedResult;
        }

        private Task<CompiledViewDescriptor> CreateCacheEntry(string normalizedPath)
        {
            TaskCompletionSource<CompiledViewDescriptor> compilationTaskSource = null;
            MemoryCacheEntryOptions cacheEntryOptions;
            Task<CompiledViewDescriptor> cacheEntry;

            // Safe races cannot be allowed when compiling Razor pages. To ensure only one compilation request succeeds
            // per file, we'll lock the creation of a cache entry. Creating the cache entry should be very quick. The
            // actual work for compiling files happens outside the critical section.
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(normalizedPath, out cacheEntry))
                {
                    return cacheEntry;
                }

                cacheEntryOptions = new MemoryCacheEntryOptions();

                cacheEntryOptions.ExpirationTokens.Add(_fileProvider.Watch(normalizedPath));
                var projectItem = _templateEngine.Project.GetItem(normalizedPath);
                if (!projectItem.Exists)
                {
                    cacheEntry = Task.FromResult(new CompiledViewDescriptor
                    {
                        RelativePath = normalizedPath,
                        ExpirationTokens = cacheEntryOptions.ExpirationTokens,
                    });
                }
                else
                {
                    // A file exists and needs to be compiled.
                    compilationTaskSource = new TaskCompletionSource<CompiledViewDescriptor>();
                    foreach (var importItem in _templateEngine.GetImportItems(projectItem))
                    {
                        cacheEntryOptions.ExpirationTokens.Add(_fileProvider.Watch(importItem.FilePath));
                    }
                    cacheEntry = compilationTaskSource.Task;
                }

                cacheEntry = _cache.Set(normalizedPath, cacheEntry, cacheEntryOptions);
            }

            if (compilationTaskSource != null)
            {
                // Indicates that a file was found and needs to be compiled.
                Debug.Assert(cacheEntryOptions != null);

                try
                {
                    var descriptor = CompileAndEmit(normalizedPath);
                    descriptor.ExpirationTokens = cacheEntryOptions.ExpirationTokens;
                    compilationTaskSource.SetResult(descriptor);
                }
                catch (Exception ex)
                {
                    compilationTaskSource.SetException(ex);
                }
            }

            return cacheEntry;
        }

        protected virtual CompiledViewDescriptor CompileAndEmit(string relativePath)
        {
            var codeDocument = _templateEngine.CreateCodeDocument(relativePath);
            var cSharpDocument = _templateEngine.GenerateCode(codeDocument);

            if (cSharpDocument.Diagnostics.Count > 0)
            {
                throw CompilationFailedExceptionFactory.Create(
                    codeDocument,
                    cSharpDocument.Diagnostics);
            }

            var generatedAssembly = CompileAndEmit(codeDocument, cSharpDocument.GeneratedCode);
            var viewAttribute = generatedAssembly.GetCustomAttribute<RazorViewAttribute>();
            return new CompiledViewDescriptor
            {
                ViewAttribute = viewAttribute,
                RelativePath = relativePath,
            };
        }

        internal Assembly CompileAndEmit(RazorCodeDocument codeDocument, string generatedCode)
        {
            _logger.GeneratedCodeToAssemblyCompilationStart(codeDocument.Source.FilePath);

            var startTimestamp = _logger.IsEnabled(LogLevel.Debug) ? Stopwatch.GetTimestamp() : 0;

            var assemblyName = Path.GetRandomFileName();
            var compilation = CreateCompilation(generatedCode, assemblyName);

            using (var assemblyStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var result = compilation.Emit(
                    assemblyStream,
                    pdbStream,
                    options: _csharpCompiler.EmitOptions);

                if (!result.Success)
                {
                    throw CompilationFailedExceptionFactory.Create(
                        codeDocument,
                        generatedCode,
                        assemblyName,
                        result.Diagnostics);
                }

                assemblyStream.Seek(0, SeekOrigin.Begin);
                pdbStream.Seek(0, SeekOrigin.Begin);

                var assembly = Assembly.Load(assemblyStream.ToArray(), pdbStream.ToArray());
                _logger.GeneratedCodeToAssemblyCompilationEnd(codeDocument.Source.FilePath, startTimestamp);

                return assembly;
            }
        }

        private CSharpCompilation CreateCompilation(string compilationContent, string assemblyName)
        {
            var sourceText = SourceText.From(compilationContent, Encoding.UTF8);
            var syntaxTree = _csharpCompiler.CreateSyntaxTree(sourceText).WithFilePath(assemblyName);
            var compilation = _csharpCompiler
                .CreateCompilation(assemblyName)
                .AddSyntaxTrees(syntaxTree);
            compilation = ExpressionRewriter.Rewrite(compilation);

            var compilationContext = new RoslynCompilationContext(compilation);
            _compilationCallback(compilationContext);
            compilation = compilationContext.Compilation;
            return compilation;
        }

        private string GetNormalizedPath(string relativePath)
        {
            Debug.Assert(relativePath != null);
            if (relativePath.Length == 0)
            {
                return relativePath;
            }

            if (!_normalizedPathLookup.TryGetValue(relativePath, out var normalizedPath))
            {
                normalizedPath = ViewPath.NormalizePath(relativePath);
                _normalizedPathLookup[relativePath] = normalizedPath;
            }

            return normalizedPath;
        }


        private static class CompilationFailedExceptionFactory
        {
            // error CS0234: The type or namespace name 'C' does not exist in the namespace 'N' (are you missing
            // an assembly reference?)
            private const string CS0234 = nameof(CS0234);
            // error CS0246: The type or namespace name 'T' could not be found (are you missing a using directive
            // or an assembly reference?)
            private const string CS0246 = nameof(CS0246);

            public static CompilationFailedException Create(
                RazorCodeDocument codeDocument,
                IEnumerable<RazorDiagnostic> diagnostics)
            {
                // If a SourceLocation does not specify a file path, assume it is produced from parsing the current file.
                var messageGroups = diagnostics.GroupBy(
                    razorError => razorError.Span.FilePath ?? codeDocument.Source.FilePath,
                    StringComparer.Ordinal);

                var failures = new List<CompilationFailure>();
                foreach (var group in messageGroups)
                {
                    var filePath = group.Key;
                    var fileContent = ReadContent(codeDocument, filePath);
                    var compilationFailure = new CompilationFailure(
                        filePath,
                        fileContent,
                        compiledContent: string.Empty,
                        messages: group.Select(parserError => CreateDiagnosticMessage(parserError, filePath)));
                    failures.Add(compilationFailure);
                }

                return new CompilationFailedException(failures);
            }

            public static CompilationFailedException Create(
                RazorCodeDocument codeDocument,
                string compilationContent,
                string assemblyName,
                IEnumerable<Diagnostic> diagnostics)
            {
                var diagnosticGroups = diagnostics
                    .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                    .GroupBy(diagnostic => GetFilePath(codeDocument, diagnostic), StringComparer.Ordinal);

                var failures = new List<CompilationFailure>();
                foreach (var group in diagnosticGroups)
                {
                    var sourceFilePath = group.Key;
                    string sourceFileContent;
                    if (string.Equals(assemblyName, sourceFilePath, StringComparison.Ordinal))
                    {
                        // The error is in the generated code and does not have a mapping line pragma
                        sourceFileContent = compilationContent;
                        sourceFilePath = "Generated Code";
                    }
                    else
                    {
                        sourceFileContent = ReadContent(codeDocument, sourceFilePath);
                    }

                    string additionalMessage = null;
                    if (group.Any(g =>
                        string.Equals(CS0234, g.Id, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(CS0246, g.Id, StringComparison.OrdinalIgnoreCase)))
                    {
                        additionalMessage = "Dependency context not specified: Microsoft.NET.Sdk.Web, PreserveCompilationContext";
                    }

                    var compilationFailure = new CompilationFailure(
                        sourceFilePath,
                        sourceFileContent,
                        compilationContent,
                        group.Select(GetDiagnosticMessage),
                        additionalMessage);

                    failures.Add(compilationFailure);
                }

                return new CompilationFailedException(failures);
            }

            private static string ReadContent(RazorCodeDocument codeDocument, string filePath)
            {
                RazorSourceDocument sourceDocument;
                if (string.IsNullOrEmpty(filePath) || string.Equals(codeDocument.Source.FilePath, filePath, StringComparison.Ordinal))
                {
                    sourceDocument = codeDocument.Source;
                }
                else
                {
                    sourceDocument = codeDocument.Imports.FirstOrDefault(f => string.Equals(f.FilePath, filePath, StringComparison.Ordinal));
                }

                if (sourceDocument != null)
                {
                    var contentChars = new char[sourceDocument.Length];
                    sourceDocument.CopyTo(0, contentChars, 0, sourceDocument.Length);
                    return new string(contentChars);
                }

                return string.Empty;
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

            private static DiagnosticMessage CreateDiagnosticMessage(
                RazorDiagnostic razorDiagnostic,
                string filePath)
            {
                var sourceSpan = razorDiagnostic.Span;
                var message = razorDiagnostic.GetMessage();
                return new DiagnosticMessage(
                    message: message,
                    formattedMessage: razorDiagnostic.ToString(),
                    filePath: filePath,
                    startLine: sourceSpan.LineIndex + 1,
                    startColumn: sourceSpan.CharacterIndex,
                    endLine: sourceSpan.LineIndex + 1,
                    endColumn: sourceSpan.CharacterIndex + sourceSpan.Length);
            }

            private static string GetFilePath(RazorCodeDocument codeDocument, Diagnostic diagnostic)
            {
                if (diagnostic.Location == Location.None)
                {
                    return codeDocument.Source.FilePath;
                }

                return diagnostic.Location.GetMappedLineSpan().Path;
            }
        }
    }
}