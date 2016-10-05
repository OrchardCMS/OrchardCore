using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Orchard.FileSystem;

namespace Orchard.Hosting.FileSystem
{
    public class HostedFileSystem : OrchardFileSystem
    {
        public HostedFileSystem(IHostingEnvironment hostingEnvironment,
            ILogger<HostedFileSystem> logger) :
            base(
                hostingEnvironment.ContentRootPath,
                hostingEnvironment.ContentRootFileProvider,
                logger)
        { }
    }
}