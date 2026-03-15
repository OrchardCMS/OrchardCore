using System.Runtime.CompilerServices;

namespace OrchardCore.Tests.Functional.Helpers;

/// <summary>
/// Runs once when the test assembly is first loaded — before xUnit constructs
/// test classes and evaluates <see cref="ServiceFactAttribute"/> skip conditions.
/// Starts Docker containers and sets environment variables so that
/// Redis/Azurite tests are not skipped when Docker is available.
/// </summary>
internal static class TestModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        AppLifecycleHelper.TryStartDockerServices();
    }
}
