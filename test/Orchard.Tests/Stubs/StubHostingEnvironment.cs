using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Orchard.Tests.Stubs
{
    public class StubHostingEnvironment : IHostingEnvironment
    {
        private string _rootPath;
        private IFileProvider _contentRootFileProvider;

        public StubHostingEnvironment(string root)
        {
            _rootPath = root;
            _contentRootFileProvider = new PhysicalFileProvider(root);
        }

        public string EnvironmentName { get; set; } = "Stub";

        public string ApplicationName { get; set; }

        public string WebRootPath { get; set; }

        public IFileProvider WebRootFileProvider { get; set; }

        public string ContentRootPath
        {
            get { return _rootPath; }
            set
            {
                _contentRootFileProvider = new PhysicalFileProvider(value);
                _rootPath = value;
            }
        }
        public IFileProvider ContentRootFileProvider
        {
            get { return _contentRootFileProvider; }
            set { _contentRootFileProvider = value; }
        }
    }
}