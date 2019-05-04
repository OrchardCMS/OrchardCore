using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace OrchardCore.Media.Services
{
    public class MediaFileProvider : PhysicalFileProvider, IMediaFileProvider
    {
        public MediaFileProvider(string root) : base(root) { }
        public MediaFileProvider(string root, ExclusionFilters filters) : base(root, filters) { }
    }
}
