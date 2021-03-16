using System.Threading;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.FileProviders;

namespace OrchardCore.Benchmark.Support
{
    internal class FakeFileProvider : IFileProvider
    {
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return new NotFoundDirectoryContents();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return new ContentFileInfo("name", "content");
        }

        public IChangeToken Watch(string filter)
        {
            return new CancellationChangeToken(CancellationToken.None);
        }
    }
}
