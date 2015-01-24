using OrchardVNext.Environment;

namespace OrchardVNext.FileSystems.AppData {
    /// <summary>
    /// Abstraction over the root location of "~/App_Data", mainly to enable
    /// unit testing of AppDataFolder.
    /// </summary>
    public interface IAppDataFolderRoot : ISingletonDependency {
        /// <summary>
        /// Virtual path of root ("~/App_Data")
        /// </summary>
        string RootPath { get; }
        /// <summary>
        /// Physical path of root (typically: MapPath(RootPath))
        /// </summary>
        string RootFolder { get; }
    }

    public class AppDataFolderRoot : IAppDataFolderRoot {
        private readonly IHostEnvironment _hostEnvironment;
        public AppDataFolderRoot(IHostEnvironment hostEnvironment) {
            _hostEnvironment = hostEnvironment;
        }

        public string RootPath => "~/App_Data";

        public string RootFolder => _hostEnvironment.MapPath(RootPath);
    }
}