using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Compilation;
using Microsoft.DotNet.ProjectModel.Files;

namespace Orchard.Environment.Extensions.ProjectModel
{
    public static class ProjectContextExtensions
    {
        public static IEnumerable<string> GetCompilationSources(this ProjectContext project, CommonCompilerOptions compilerOptions)
        {
            if (compilerOptions.CompileInclude == null)
            {
                return project.ProjectFile.Files.SourceFiles;
            }

            var includeFiles = IncludeFilesResolver.GetIncludeFiles(compilerOptions.CompileInclude, "/", diagnostics: null);

            return includeFiles.Select(f => f.SourcePath);
        }
    }

    public static class LibraryExporterExtensions
    {
        public static IEnumerable<LibraryExport> GetAllCompatibleExports(this LibraryExporter exporter)
        {
            return exporter.GetAllExports().Where(export => export.Library.Compatible);
        }
    }

    public static class LibraryExportExtensions
    {
        public static IEnumerable<string> GetSourceReferences(this LibraryExport export, string tempLocation, string tempName = null)
        {
            return export.SourceReferences.Select(s => s.GetTransformedFile(tempLocation, tempName));
        }
    }
}
