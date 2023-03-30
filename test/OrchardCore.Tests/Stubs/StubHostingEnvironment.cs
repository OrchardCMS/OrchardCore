namespace OrchardCore.Tests.Stubs
{
    public class StubHostingEnvironment : IHostEnvironment
    {
        private string _rootPath;
        private IFileProvider _contentRootFileProvider;

        public StubHostingEnvironment()
        {
            ApplicationName = GetType().Assembly.GetName().Name;
        }

        public string EnvironmentName { get; set; } = "Stub";

        public string ApplicationName { get; set; }

        public string WebRootPath { get; set; }

        public IFileProvider WebRootFileProvider { get; set; }

        public string ContentRootPath
        {
            get { return _rootPath ?? Directory.GetCurrentDirectory(); }
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
