using Microsoft.Dnx.Compilation;

namespace Microsoft.Dnx.Runtime {
    public static class ProjectFilesCollectionExtensions {
        internal static CompilationFiles GetCompilationFiles(this ProjectFilesCollection collection) {
            return new CompilationFiles(
                collection.PreprocessSourceFiles,
                collection.SourceFiles);
        }
    }
}