using Microsoft.Extensions.PlatformAbstractions;
using Orchard.Environment;

namespace Orchard.Hosting
{
    public abstract class HostEnvironment : IHostEnvironment
    {
        private readonly IApplicationEnvironment _applicationEnvrionment;

        protected HostEnvironment(IApplicationEnvironment applicationEnvrionment)
        {
            _applicationEnvrionment = applicationEnvrionment;
        }

        public string MapPath(string virtualPath)
        {
            return _applicationEnvrionment.ApplicationBasePath + virtualPath.Replace("~/", "\\");
        }
    }
}