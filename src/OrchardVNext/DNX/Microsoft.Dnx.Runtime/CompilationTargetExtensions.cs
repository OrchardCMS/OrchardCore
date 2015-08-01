using System.Runtime.Versioning;

namespace Microsoft.Dnx.Runtime {
    internal static class CompilationTargetExtensions {
        public static CompilationTarget ChangeName(this CompilationTarget target, string name) {
            return new CompilationTarget(name, target.TargetFramework, target.Configuration, target.Aspect);
        }

        public static CompilationTarget ChangeTargetFramework(this CompilationTarget target, FrameworkName targetFramework) {
            return new CompilationTarget(target.Name, targetFramework, target.Configuration, target.Aspect);
        }

        public static CompilationTarget ChangeAspect(this CompilationTarget target, string aspect) {
            return new CompilationTarget(target.Name, target.TargetFramework, target.Configuration, aspect);
        }
    }
}