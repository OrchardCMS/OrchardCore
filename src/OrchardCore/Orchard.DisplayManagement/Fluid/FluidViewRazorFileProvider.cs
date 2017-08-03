using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Orchard.DisplayManagement.FileProviders;
using Orchard.DisplayManagement.Fluid.Internal;

namespace Orchard.DisplayManagement.Fluid
{
    public interface IFluidViewRazorFileProvider : IFileProvider { }

    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides a custom razor content for fluid views.
    /// </summary>
    public class FluidViewRazorFileProvider : IFluidViewRazorFileProvider
    {
        private readonly IFluidViewFileProviderAccessor _fileProviderAccessor;

        public FluidViewRazorFileProvider(IFluidViewFileProviderAccessor fileProviderAccessor)
        {
            _fileProviderAccessor = fileProviderAccessor;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return null;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var fileInfo = _fileProviderAccessor.ShellFileProvider.GetFileInfo(Path.ChangeExtension(subpath, FluidViewTemplate.ViewExtension));

            if (fileInfo != null && fileInfo.Exists)
            {
                return new ContentFileInfo(Path.GetFileName(subpath), FluidPage.Content);
            }

            return null;
        }

        public IChangeToken Watch(string filter)
        {
            return _fileProviderAccessor.ShellFileProvider.Watch(Path.ChangeExtension(filter, FluidViewTemplate.ViewExtension));
        }
    }
}
