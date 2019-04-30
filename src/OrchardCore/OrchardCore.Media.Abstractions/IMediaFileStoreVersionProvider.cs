using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.FileStorage;

namespace OrchardCore.Media
{
    public interface IMediaFileStoreVersionProvider
    {
        Task<string> AddFileVersionToPathAsync(PathString requestPathBase, string path);
    }
}
