using OrchardVNext.FileSystems.VirtualPath;

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
        private readonly IVirtualPathProvider _virtualPathProvider;
        public AppDataFolderRoot(IVirtualPathProvider virtualPathProvider) {
            _virtualPathProvider = virtualPathProvider;
        }

        public string RootPath {
            get { return "~/App_Data"; }
        }

        public string RootFolder {
            get {
                return _virtualPathProvider.MapPath(RootPath);
            }
        }
    }
}