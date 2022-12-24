using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.Testing.Stubs;

public class HostingEnvironmentStub : IHostEnvironment
{
    private string _rootPath;
    private IFileProvider _contentRootFileProvider;

    public HostingEnvironmentStub()
    {
        ApplicationName = GetType().Assembly.GetName().Name;
    }

    public string EnvironmentName { get; set; } = "Testing";

    public string ApplicationName { get; set; }

    public string WebRootPath { get; set; }

    public IFileProvider WebRootFileProvider { get; set; }

    public string ContentRootPath
    {
        get => _rootPath ?? Directory.GetCurrentDirectory();
        set
        {
            _contentRootFileProvider = new PhysicalFileProvider(value);
            _rootPath = value;
        }
    }

    public IFileProvider ContentRootFileProvider
    {
        get => _contentRootFileProvider;
        set => _contentRootFileProvider = value;
    }
}
