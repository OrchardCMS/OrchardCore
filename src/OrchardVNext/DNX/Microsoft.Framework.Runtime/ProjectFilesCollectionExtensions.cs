using Microsoft.Framework.Runtime.Compilation;

namespace Microsoft.Framework.Runtime {
    public static class ProjectFilesCollectionExtensions {
        internal static CompilationFiles GetCompilationFiles(this ProjectFilesCollection collection) {
            return new CompilationFiles(
                collection.PreprocessSourceFiles,
                collection.SourceFiles);
        }
    }
}