using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Scripting;

public interface IScriptingEngine
{
    string Prefix { get; }
    object Evaluate(IScriptingScope scope, string script);

    /// <summary>
    /// Creates a scripting scope with file context for engines that support file operations.
    /// </summary>
    /// <param name="methods">The global methods to make available in the scope.</param>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <param name="fileProvider">The file provider for file operations, or <c>null</c> if not available.</param>
    /// <param name="basePath">The base path for file resolution, or <c>null</c> if not available.</param>
    /// <returns>A scripting scope.</returns>
    IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath);

    /// <summary>
    /// Creates a scripting scope without file context.
    /// </summary>
    /// <param name="methods">The global methods to make available in the scope.</param>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <returns>A scripting scope.</returns>
    IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider)
        => CreateScope(methods, serviceProvider, null, null);
}
