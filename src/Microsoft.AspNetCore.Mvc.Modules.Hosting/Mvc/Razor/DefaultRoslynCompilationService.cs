using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orchard.Hosting.Mvc.Razor
{
    /// <summary>
    /// A type that uses Roslyn to compile C# content.
    /// </summary>
    public class DefaultRoslynCompilationService : ICompilationService
    {
        private readonly Object _syncLock = new Object();

        // error CS0234: The type or namespace name 'C' does not exist in the namespace 'N' (are you missing
        // an assembly reference?)
        private const string CS0234 = nameof(CS0234);
        // error CS0246: The type or namespace name 'T' could not be found (are you missing a using directive
        // or an assembly reference?)
        private const string CS0246 = nameof(CS0246);

        private readonly CSharpCompiler _compiler;
        private readonly IFileProvider _fileProvider;
        private readonly ILogger _logger;
        private readonly Action<RoslynCompilationContext> _compilationCallback;

        /// <summary>
        /// Initalizes a new instance of the <see cref="DefaultRoslynCompilationService"/> class.
        /// </summary>
        /// <param name="compiler">The <see cref="CSharpCompiler"/>.</param>
        /// <param name="optionsAccessor">Accessor to <see cref="RazorViewEngineOptions"/>.</param>
        /// <param name="fileProviderAccessor">The <see cref="IRazorViewEngineFileProviderAccessor"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public DefaultRoslynCompilationService(
            CSharpCompiler compiler,
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            IOptions<RazorViewEngineOptions> optionsAccessor,
            ILoggerFactory loggerFactory)
        {
            _compiler = compiler;
            _fileProvider = fileProviderAccessor.FileProvider;
            _compilationCallback = optionsAccessor.Value.CompilationCallback;
            _logger = loggerFactory.CreateLogger<DefaultRoslynCompilationService>();
        }

        /// <inheritdoc />
        public CompilationResult Compile(RelativeFileInfo fileInfo, string compilationContent)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            string pdbPath = null;
            string assemblyPath = null;
            if (!fileInfo.FileInfo.Name.StartsWith("_"))
            {
                var name = Path.GetFileNameWithoutExtension(fileInfo.FileInfo.Name) + ".Precompiled";
                var assemblyFolderPath = Path.GetDirectoryName(fileInfo.FileInfo.PhysicalPath);
                assemblyPath = Path.Combine(assemblyFolderPath, name + ".dll");
                pdbPath = Path.Combine(assemblyFolderPath, name + ".pdb");
            }

            Type type;
            Assembly assembly;
            if (assemblyPath == null || !File.Exists(assemblyPath) || fileInfo.FileInfo.LastModified
                > File.GetLastWriteTimeUtc(assemblyPath))
            {

                if (compilationContent == null)
                {
                    throw new ArgumentNullException(nameof(compilationContent));
                }

                var assemblyName = Path.GetRandomFileName();
                var compilation = CreateCompilation(compilationContent, assemblyName);

                using (var assemblyStream = new MemoryStream())
                {
                    using (var pdbStream = new MemoryStream())
                    {
                        var result = compilation.Emit(
                            assemblyStream,
                            pdbStream,
                            options: _compiler.EmitOptions);

                        if (!result.Success)
                        {
                            return GetCompilationFailedResult(
                                fileInfo.RelativePath,
                                compilationContent,
                                assemblyName,
                                result.Diagnostics);
                        }

                        assemblyStream.Seek(0, SeekOrigin.Begin);
                        pdbStream.Seek(0, SeekOrigin.Begin);

                        assembly = LoadAssembly(assemblyStream, pdbStream);
                        type = assembly.GetExportedTypes().FirstOrDefault(a => !a.IsNested);

                        if (assemblyPath != null)
                        {
                            lock (_syncLock)
                            {
                                if (!File.Exists(assemblyPath) || fileInfo.FileInfo.LastModified
                                    > File.GetLastWriteTimeUtc(assemblyPath))
                                {
                                    assemblyStream.Seek(0, SeekOrigin.Begin);
                                    pdbStream.Seek(0, SeekOrigin.Begin);

                                    using (var assemblyFileStream = File.OpenWrite(assemblyPath))
                                    {
                                        assemblyStream.CopyTo(assemblyFileStream);
                                    }

                                    using (var pdbFileStream = File.OpenWrite(pdbPath))
                                    {
                                        pdbStream.CopyTo(pdbFileStream);
                                    }
                                }
                            }
                        }

                        return new CompilationResult(type);
                    }
                }
            }
            else
            {
                using (var assemblyFileStream = File.OpenRead(assemblyPath))
                {
                    using (var pdbFileStream = File.OpenRead(pdbPath))
                    {
                        using (var assemblyStream = new MemoryStream())
                        {
                            using (var pdbStream = new MemoryStream())
                            {
                                assemblyFileStream.Seek(0, SeekOrigin.Begin);
                                pdbFileStream.Seek(0, SeekOrigin.Begin);

                                assemblyFileStream.CopyTo(assemblyStream);
                                pdbFileStream.CopyTo(pdbStream);

                                assemblyStream.Seek(0, SeekOrigin.Begin);
                                pdbStream.Seek(0, SeekOrigin.Begin);

                                assembly = LoadAssembly(assemblyStream, pdbStream);
                                type = assembly.GetExportedTypes().FirstOrDefault(a => !a.IsNested);
                            }
                        }
                    }
                }
            }

            return new CompilationResult(type);
        }

        private CSharpCompilation CreateCompilation(string compilationContent, string assemblyName)
        {
            var sourceText = SourceText.From(compilationContent, Encoding.UTF8);
            var syntaxTree = _compiler.CreateSyntaxTree(sourceText).WithFilePath(assemblyName);
            var compilation = _compiler
                .CreateCompilation(assemblyName)
                .AddSyntaxTrees(syntaxTree);
            compilation = ExpressionRewriter.Rewrite(compilation);

            var compilationContext = new RoslynCompilationContext(compilation);
            _compilationCallback(compilationContext);
            compilation = compilationContext.Compilation;
            return compilation;
        }

        // Internal for unit testing
        internal CompilationResult GetCompilationFailedResult(
            string relativePath,
            string compilationContent,
            string assemblyName,
            IEnumerable<Diagnostic> diagnostics)
        {
            var diagnosticGroups = diagnostics
                .Where(IsError)
                .GroupBy(diagnostic => GetFilePath(relativePath, diagnostic), StringComparer.Ordinal);

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
                    sourceFileContent = ReadFileContentsSafely(_fileProvider, sourceFilePath);
                }

                string additionalMessage = null;
                if (group.Any(g =>
                    string.Equals(CS0234, g.Id, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(CS0246, g.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    additionalMessage = string.Format(CultureInfo.CurrentCulture,
                        "One or more compilation references are missing. Possible causes include a missing '{0}' property under '{1}' in the application's {2}.",
                        "preserveCompilationContext", "buildOptions", "project.json");
                }

                var compilationFailure = new CompilationFailure(
                    sourceFilePath,
                    sourceFileContent,
                    compilationContent,
                    group.Select(GetDiagnosticMessage),
                    additionalMessage);

                failures.Add(compilationFailure);
            }

            return new CompilationResult(failures);
        }

        private static string GetFilePath(string relativePath, Diagnostic diagnostic)
        {
            if (diagnostic.Location == Location.None)
            {
                return relativePath;
            }

            return diagnostic.Location.GetMappedLineSpan().Path;
        }

        private static bool IsError(Diagnostic diagnostic)
        {
            return diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error;
        }

        public static Assembly LoadAssembly(MemoryStream assemblyStream, MemoryStream pdbStream)
        {
            var assembly =
#if NET451
                Assembly.Load(assemblyStream.ToArray(), pdbStream.ToArray());
#else
                System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(assemblyStream, pdbStream);
#endif
            return assembly;
        }

        private static string ReadFileContentsSafely(IFileProvider fileProvider, string filePath)
        {
            var fileInfo = fileProvider.GetFileInfo(filePath);
            if (fileInfo.Exists)
            {
                try
                {
                    using (var reader = new StreamReader(fileInfo.CreateReadStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
                catch
                {
                    // Ignore any failures
                }
            }

            return null;
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
    }
}