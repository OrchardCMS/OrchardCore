using System.Reflection;
using System.Runtime.CompilerServices;
using OrchardCore.Localization.Extraction;

namespace OrchardCore.Localization.Extraction.Tests;

/// <summary>
/// Binds the test process to the compiler of the SDK that built the tests, as the build-time worker does, before any
/// test touches a compiler type.
/// </summary>
internal static class SdkTestInitializer
{
    public static string RoslynDirectory { get; } = GetMetadata("RoslynDirectory");
    public static string RazorCompilerPath { get; } = GetMetadata("RazorCompilerPath");

    [ModuleInitializer]
    public static void Initialize()
        => SdkAssemblyResolver.Register(RoslynDirectory, Path.GetDirectoryName(RazorCompilerPath));

    private static string GetMetadata(string key)
        => typeof(SdkTestInitializer).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(attribute => attribute.Key == key)?.Value ?? "";
}
