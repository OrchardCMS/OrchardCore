using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Orchard.Environment.Shell
{
    public class ShellOptions
    {
        public string Location { get; set; }

        public IFileInfo Shell {
            get {
                return AppDataFileProvider.GetFileInfo(Location);
            }
        }

        public IDirectoryContents ShellSettings
        {
            get
            {
                return AppDataFileProvider.GetDirectoryContents(Location);
            }
        }

        private IFileProvider AppDataFileProvider
        {
            get
            {
                return
                    new PhysicalFileProvider(
                        Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "app_data");
            }
        }
    }
}