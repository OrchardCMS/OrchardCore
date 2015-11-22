using Orchard.Environment;

namespace Orchard.FileSystem.AppData
{
    public class AppDataFolderRoot : IAppDataFolderRoot
    {
        private readonly IHostEnvironment _hostEnvironment;
        public AppDataFolderRoot(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public string RootPath => "~/App_Data";

        public string RootFolder => _hostEnvironment.MapPath(RootPath);
    }
}