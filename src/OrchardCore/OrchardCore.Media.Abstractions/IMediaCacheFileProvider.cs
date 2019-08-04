using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Media
{
    /// <summary>
    /// A custom file provider used to cache media per tenant
    /// </summary>
    public interface IMediaCacheFileProvider : IFileProvider
    {
    }
}
