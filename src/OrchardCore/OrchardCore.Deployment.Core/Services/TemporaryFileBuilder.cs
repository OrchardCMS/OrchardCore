using System;
using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Deployment.Core.Services
{
    public sealed class TemporaryFileBuilder : IFileBuilder, IDisposable
    {
        private readonly bool _deleteOnDispose;

        public TemporaryFileBuilder(bool deleteOnDispose = true)
        {
            Folder = PathExtensions.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _deleteOnDispose = deleteOnDispose;
        }

        public string Folder { get; }

        public void Dispose()
        {
            if (_deleteOnDispose)
            {
                if (Directory.Exists(Folder))
                {
                    Directory.Delete(Folder, true);
                }
            }
        }

        public async Task SetFileAsync(string subpath, Stream stream)
        {
            if (subpath.StartsWith('/'))
            {
                throw new InvalidOperationException("A virtual path is required");
            }

            var fullname = PathExtensions.Combine(Folder, subpath);

            var directory = new FileInfo(fullname).Directory;

            if (!directory.Exists)
            {
                directory.Create();
            }

            using var fs = File.Create(fullname, 4 * 1024, FileOptions.None);
            await stream.CopyToAsync(fs);
        }

        public override string ToString()
        {
            return Folder;
        }
    }
}
