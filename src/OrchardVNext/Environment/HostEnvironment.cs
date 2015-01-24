using Microsoft.AspNet.Hosting;

namespace OrchardVNext.Environment {
    public abstract class HostEnvironment : IHostEnvironment {
        private readonly IHostingEnvironment _hostingEnvrionment;
        public HostEnvironment(IHostingEnvironment hostingEnvrionment) {
            _hostingEnvrionment = hostingEnvrionment;
        }

        public string MapPath(string virtualPath) {
            return _hostingEnvrionment.WebRoot + virtualPath.Replace("~", string.Empty).Replace('/', '\\');
        }
    }
}