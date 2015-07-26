using Microsoft.CodeAnalysis;

namespace Microsoft.Framework.Runtime.Roslyn {
    internal class RoslynDiagnostics {
        internal static readonly DiagnosticDescriptor StrongNamingNotSupported = new DiagnosticDescriptor(
            id: "DNX1001",
            title: "Strong name generation is not supported on this platform",
            messageFormat: "Strong name generation is not supported on CoreCLR. Skipping strong name generation.",
            category: "StrongNaming",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}