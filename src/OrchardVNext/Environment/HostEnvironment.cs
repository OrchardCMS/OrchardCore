using Microsoft.Dnx.Runtime;

namespace OrchardVNext.Environment {
    public abstract class HostEnvironment : IHostEnvironment {
        private readonly IApplicationEnvironment _applicationEnvrionment;

        protected HostEnvironment(IApplicationEnvironment applicationEnvrionment) {
            _applicationEnvrionment = applicationEnvrionment;
        }

        public string MapPath(string virtualPath) {
       		return _applicationEnvrionment.ApplicationBasePath + virtualPath.Replace("~", string.Empty);
        }
    }
}