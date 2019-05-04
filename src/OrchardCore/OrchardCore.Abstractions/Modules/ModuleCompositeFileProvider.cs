using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides Di registration identification
    /// for IFileProviders that should be served via UseStaticFiles.
    /// </summary>
    public class ModuleCompositeFileProvider : CompositeFileProvider, IModuleStaticFileProvider
    {
        public ModuleCompositeFileProvider(params IFileProvider[] fileProviders) : base(fileProviders) { }
        public ModuleCompositeFileProvider(IEnumerable<IFileProvider> fileProviders) : base(fileProviders) { }
    }
}
