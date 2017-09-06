using System;
using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Deployment.Core.Services
{
    public class TemporaryFileBuilder : IFileBuilder, IDisposable
    {
        private readonly bool _deleteOnDispose;

        public TemporaryFileBuilder(bool deleteOnDispose = true)
        {
            Folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
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

        public async Task SetFileAsync(string subpath, byte[] content)
        {
            var fullname = Path.Combine(Folder, subpath);

            var directory = new FileInfo(fullname).Directory;

            if (!directory.Exists)
            {
                directory.Create();
            }

            using (var fs = File.Create(fullname, 4 * 1024, FileOptions.None))
            {
                await fs.WriteAsync(content, 0, content.Length);
            }
        }

        public override string ToString()
        {
            return Folder;
        }
    }
}
