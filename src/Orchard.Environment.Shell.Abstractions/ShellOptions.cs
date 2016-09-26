using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Orchard.Environment.Shell
{
    public class ShellOptions
    {
        public IFileProvider ContentRootFileProvider { get; set; }

        public string ShellContainerLocation { private get; set; }

        public IFileInfo ShellHostContainer
        {
            get
            {
                return ContentRootFileProvider.GetFileInfo("app_data");
            }
        }

        public IFileInfo ShellContainer
        {
            get
            {
                return ContentRootFileProvider.GetFileInfo(Path.Combine("app_data", ShellContainerLocation));
            }
        }

        public IDirectoryContents Shells
        {
            get
            {
                return ContentRootFileProvider.GetDirectoryContents(Path.Combine("app_data", ShellContainer.Name));
            }
        }
    }
}