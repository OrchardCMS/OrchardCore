using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.Compiler.Common;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Files;
using Microsoft.DotNet.ProjectModel.Resources;
using Microsoft.DotNet.Tools.Common;
using NuGet.Frameworks;

namespace Orchard.Environment.Extensions.Compilers
{
    public static class CompilerUtility
    {
        public const string RefsDirectoryName = "refs";
        public const string ReleaseConfiguration = "Release";
        public const string DefaultConfiguration = Constants.DefaultConfiguration;
        public const string LocaleLockFilePropertyName = "locale";

        public static string ResolveAssetPath(string binaryFolderPath, string probingFolderPath, string assetFileName, string relativeFolderPath = null)
        {
            binaryFolderPath = !String.IsNullOrEmpty(relativeFolderPath)
                ? Path.Combine(binaryFolderPath, relativeFolderPath)
                : binaryFolderPath;

            probingFolderPath = !String.IsNullOrEmpty(relativeFolderPath)
                ? Path.Combine(probingFolderPath, relativeFolderPath)
                : probingFolderPath;

            var binaryPath = Path.Combine(binaryFolderPath, assetFileName);
            var probingPath = Path.Combine(probingFolderPath, assetFileName);

            return Files.GetNewest(binaryPath, probingPath);
        }

        public static string GetAssemblyFileName(string assemblyName)
        {
            return assemblyName + FileNameSuffixes.DotNet.DynamicLib;
        }

        public static string GetAssemblyFolderPath(string rootPath, string config, string framework)
        {
            return Path.Combine(rootPath, Constants.BinDirectoryName, config, NuGetFramework.Parse(framework).GetShortFolderName());
        }

        public static IEnumerable<string> GetOtherParentProjectsLocations(ProjectContext context, LibraryDescription library)
        {
            foreach (var parent in library.Parents)
            {
                if (parent is ProjectDescription && !parent.Identity.Name.Equals(context.ProjectName()))
                {
                    if (!string.IsNullOrEmpty(parent.Path))
                    {
                        yield return Paths.GetParentFolderPath(parent.Path);
                    }
                }

                foreach (var path in GetOtherParentProjectsLocations(context, parent))
                {
                    yield return path;
                }
            }
        }

        public static IEnumerable<string> GetCompilationSources(ProjectContext project, CommonCompilerOptions compilerOptions)
        {
            if (compilerOptions.CompileInclude == null)
            {
                return project.ProjectFile.Files.SourceFiles;
            }

            var includeFiles = IncludeFilesResolver.GetIncludeFiles(compilerOptions.CompileInclude, "/", diagnostics: null);

            return includeFiles.Select(f => f.SourcePath);
        }

        public static List<NonCultureResgenIO> GetNonCultureResources(Project project, string intermediateOutputPath, CommonCompilerOptions compilationOptions)
        {
            if (compilationOptions.EmbedInclude == null)
            {
                return GetNonCultureResources(project, intermediateOutputPath);
            }

            return GetNonCultureResourcesFromIncludeEntries(project, intermediateOutputPath, compilationOptions);
        }

        public static List<CultureResgenIO> GetCultureResources(Project project, string outputPath, CommonCompilerOptions compilationOptions)
        {
            if (compilationOptions.EmbedInclude == null)
            {
                return GetCultureResources(project, outputPath);
            }

            return GetCultureResourcesFromIncludeEntries(project, outputPath, compilationOptions);
        }

        public static bool GenerateNonCultureResources(Project project, List<CompilerUtility.NonCultureResgenIO> resgenFiles, IList<string> diagnostics)
        {
            foreach (var resgenFile in resgenFiles)
            {
                if (ResourceUtility.IsResxFile(resgenFile.InputFile))
                {
                    var outputResourceFile = ResourceFile.Create(resgenFile.OutputFile);

                    if (outputResourceFile.Type != ResourceFileType.Resources)
                    {
                        diagnostics.Add("Resource output type not supported");
                        return false;
                    }

                    using (var outputStream = outputResourceFile.File.Create())
                    {
                        var resourceSource = new ResourceSource(ResourceFile.Create(resgenFile.InputFile), resgenFile.InputFile);
                        ResourceFileGenerator.Generate(resourceSource.Resource, outputStream);
                    }
                }
            }

            return true;
        }

        public static bool GenerateCultureResourceAssemblies(Project project, List<CompilerUtility.CultureResgenIO> cultureResgenFiles, List<string> referencePaths, IList<string> diagnostics)
        {
            foreach (var resgenFile in cultureResgenFiles)
            {
                var resourceOutputPath = Paths.GetParentFolderPath(resgenFile.OutputFile);
                Directory.CreateDirectory(resourceOutputPath);

                var inputResourceFiles = resgenFile.InputFileToMetadata
                    .Select(fileToMetadata => new ResourceSource(ResourceFile.Create(fileToMetadata.Key), fileToMetadata.Value))
                    .ToArray();

                var outputResourceFile = ResourceFile.Create(resgenFile.OutputFile);

                if (outputResourceFile.Type != ResourceFileType.Dll)
                {
                    diagnostics.Add("Resource output type not supported");
                    return false;
                }

                using (var outputStream = outputResourceFile.File.Create())
                {
                    var metadata = new AssemblyInfoOptions
                    {
                        Culture = resgenFile.Culture,
                        AssemblyVersion = project.Version.Version.ToString(),
                    };

                    ResourceAssemblyGenerator.Generate(inputResourceFiles,
                        outputStream,
                        metadata,
                        Path.GetFileNameWithoutExtension(outputResourceFile.File.Name),
                        referencePaths.ToArray(),
                        diagnostics);
                }
            }

            return true;
        }

        public struct NonCultureResgenIO
        {
            public readonly string InputFile;
            public readonly string MetadataName;
            public readonly string OutputFile;

            public NonCultureResgenIO(string inputFile, string outputFile, string metadataName)
            {
                InputFile = inputFile;
                OutputFile = outputFile;
                MetadataName = metadataName;
            }
        }

        public static List<NonCultureResgenIO> GetNonCultureResources(Project project, string intermediateOutputPath)
        {
            return 
                (from resourceFile in project.Files.ResourceFiles
                 let inputFile = resourceFile.Key
                 where string.IsNullOrEmpty(ResourceUtility.GetResourceCultureName(inputFile))
                 let metadataName = GetResourceFileMetadataName(project, resourceFile.Key, resourceFile.Value)
                 let outputFile = ResourceUtility.IsResxFile(inputFile) ? Path.Combine(intermediateOutputPath, metadataName) : null
                 select new NonCultureResgenIO(inputFile, outputFile, metadataName))
                 .ToList();
        }

        public static List<NonCultureResgenIO> GetNonCultureResourcesFromIncludeEntries(Project project, string intermediateOutputPath, CommonCompilerOptions compilationOptions)
        {
            var includeFiles = IncludeFilesResolver.GetIncludeFiles(compilationOptions.EmbedInclude, "/", diagnostics: null);
            return
                (from resourceFile in includeFiles
                 let inputFile = resourceFile.SourcePath
                 where string.IsNullOrEmpty(ResourceUtility.GetResourceCultureName(inputFile))
                 let target = resourceFile.IsCustomTarget ? resourceFile.TargetPath : null
                 let metadataName = GetResourceFileMetadataName(project, resourceFile.SourcePath, target)
                 let outputFile = ResourceUtility.IsResxFile(inputFile) ? Path.Combine(intermediateOutputPath, metadataName) : null
                 select new NonCultureResgenIO(inputFile, outputFile, metadataName))
                 .ToList();
        }

        public struct CultureResgenIO
        {
            public readonly string Culture;
            public readonly Dictionary<string, string> InputFileToMetadata;
            public readonly string OutputFile;

            public CultureResgenIO(string culture, Dictionary<string, string> inputFileToMetadata, string outputFile)
            {
                Culture = culture;
                InputFileToMetadata = inputFileToMetadata;
                OutputFile = outputFile;
            }
        }

        public static List<CultureResgenIO> GetCultureResources(Project project, string outputPath)
        {
            return
                (from resourceFileGroup in project.Files.ResourceFiles.GroupBy(resourceFile => ResourceUtility.GetResourceCultureName(resourceFile.Key))
                 let culture = resourceFileGroup.Key
                 where !string.IsNullOrEmpty(culture)
                 let inputFileToMetadata = resourceFileGroup.ToDictionary(r => r.Key, r => GetResourceFileMetadataName(project, r.Key, r.Value))
                 let resourceOutputPath = Path.Combine(outputPath, culture)
                 let outputFile = Path.Combine(resourceOutputPath, project.Name + ".resources.dll")
                 select new CultureResgenIO(culture, inputFileToMetadata, outputFile))
                 .ToList();
        }

        public static List<CultureResgenIO> GetCultureResourcesFromIncludeEntries(Project project, string outputPath, CommonCompilerOptions compilationOptions)
        {
            var includeFiles = IncludeFilesResolver.GetIncludeFiles(compilationOptions.EmbedInclude, "/", diagnostics: null);
            return
                (from resourceFileGroup in includeFiles
                 .GroupBy(resourceFile => ResourceUtility.GetResourceCultureName(resourceFile.SourcePath))
                 let culture = resourceFileGroup.Key
                 where !string.IsNullOrEmpty(culture)
                 let inputFileToMetadata = resourceFileGroup.ToDictionary(
                     r => r.SourcePath, r => GetResourceFileMetadataName(project, r.SourcePath, r.IsCustomTarget ? r.TargetPath : null))
                 let resourceOutputPath = Path.Combine(outputPath, culture)
                 let outputFile = Path.Combine(resourceOutputPath, project.Name + ".resources.dll")
                 select new CultureResgenIO(culture, inputFileToMetadata, outputFile))
                 .ToList();
        }

        public static string GetResourceFileMetadataName(Project project, string resourceFileSource, string resourceFileTarget)
        {
            string resourceName = null;
            string rootNamespace = null;

            string root = PathUtility.EnsureTrailingSlash(project.ProjectDirectory);
            string resourcePath = resourceFileSource;
            if (string.IsNullOrEmpty(resourceFileTarget))
            {
                resourceName = ResourceUtility.GetResourceName(root, resourcePath);
                rootNamespace = project.Name;
            }
            else
            {
                resourceName = ResourceManifestName.EnsureResourceExtension(resourceFileTarget, resourcePath);
                rootNamespace = null;
            }

            return ResourceManifestName.CreateManifestName(resourceName, rootNamespace);
        }
    }
}