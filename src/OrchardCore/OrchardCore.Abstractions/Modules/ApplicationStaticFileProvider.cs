using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Modules;

/// <summary>
/// Provides the application's physical web-root files through its module URL prefix.
/// </summary>
public sealed class ApplicationStaticFileProvider : IStaticFileProvider
{
    private readonly string _applicationPath;
    private readonly IFileProvider _webRootFileProvider;

    public ApplicationStaticFileProvider(
        IApplicationContext applicationContext,
        IWebHostEnvironment environment)
    {
        _applicationPath = applicationContext.Application.Name + '/';
        _webRootFileProvider = environment.WebRootFileProvider;
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return NotFoundDirectoryContents.Singleton;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        if (subpath == null)
        {
            return new NotFoundFileInfo(subpath);
        }

        var path = NormalizePath(subpath);

        if (path.StartsWith(_applicationPath, StringComparison.Ordinal))
        {
            return _webRootFileProvider.GetFileInfo(path[_applicationPath.Length..]);
        }

        return new NotFoundFileInfo(subpath);
    }

    public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

    private static string NormalizePath(string path)
    {
        if (!path.Contains('\\') &&
            !path.StartsWith('/') &&
            !path.EndsWith('/') &&
            !path.Contains("//", StringComparison.Ordinal))
        {
            return path;
        }

        return path.Replace('\\', '/').Trim('/').Replace("//", "/");
    }
}
