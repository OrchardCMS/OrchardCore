using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Deployment.Core.Services;

/// <summary>Owns validated private package files until import has finished.</summary>
public sealed class StagedDeploymentPackage : IDisposable
{
    private readonly string _folder;
    private readonly string _packagePath;
    private bool _disposed;
    private readonly PhysicalFileProvider _provider;

    internal StagedDeploymentPackage(string folder, string packagePath)
    {
        _folder = folder;
        _packagePath = packagePath;
        _provider = new PhysicalFileProvider(folder);
    }

    /// <summary>Gets the validated package files for the deployment manager.</summary>
    public IFileProvider FileProvider => _provider;

    /// <summary>Opens the validated original package for persistence or transfer.</summary>
    /// <remarks>The caller must dispose the returned stream before disposing this package.</remarks>
    public Stream OpenRead()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return new FileStream(_packagePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
    }

    /// <summary>Closes file watchers and removes staged files.</summary>
    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;
        _provider.Dispose();
        File.Delete(_packagePath);
        if (Directory.Exists(_folder)) { Directory.Delete(_folder, recursive: true); }
    }
}
