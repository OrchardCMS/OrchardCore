using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Abstractions.Modules.FileProviders;

namespace OrchardCore.Media
{
    public interface IMediaFileProvider : IFileProvider, IVirtualPathBaseProvider
    {
    }
}
