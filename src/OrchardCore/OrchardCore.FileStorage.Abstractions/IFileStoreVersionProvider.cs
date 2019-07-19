using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.FileStorage
{
    public interface IFileStoreVersionProvider
    {
        Task<string> AddFileVersionToPathAsync(PathString requestPathBase, string path);
    }
}
