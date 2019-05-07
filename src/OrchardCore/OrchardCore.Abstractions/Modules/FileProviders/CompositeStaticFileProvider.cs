using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Modules.FileProviders
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides Di registration identification
    /// for IStaticFileProviders that should be served via UseStaticFiles.
    /// </summary>
    public class CompositeStaticFileProvider : CompositeFileProvider, IModuleStaticFileProvider
    {
        public CompositeStaticFileProvider(params IStaticFileProvider[] fileProviders) : base(fileProviders) { }
        public CompositeStaticFileProvider(IEnumerable<IStaticFileProvider> fileProviders) : base(fileProviders) { }
    }
}
