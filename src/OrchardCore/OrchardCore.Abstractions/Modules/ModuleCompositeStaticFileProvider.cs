using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides Di registration identification
    /// for IStaticFileProviders that should be served via UseStaticFiles.
    /// </summary>
    public class ModuleCompositeStaticFileProvider : CompositeFileProvider, IModuleStaticFileProvider
    {
        public ModuleCompositeStaticFileProvider(params IStaticFileProvider[] fileProviders) : base(fileProviders) { }
        public ModuleCompositeStaticFileProvider(IEnumerable<IStaticFileProvider> fileProviders) : base(fileProviders) { }
    }
}
