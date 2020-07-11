using System;

namespace OrchardCore.FileStorage.AzureBlob
{
    public class BlobFile : IFileStoreEntry
    {
        public BlobFile(string path, long? length, DateTimeOffset? lastModified)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(Path);

            if (Name == Path) // file is in root Directory
            {
                DirectoryPath = "";
            }
            else
            {
                DirectoryPath = Path.Substring(0, Path.Length - Name.Length - 1);
            }

            Length = length.GetValueOrDefault();
            LastModifiedUtc = lastModified.GetValueOrDefault().UtcDateTime;
        }

        public string Path { get; }

        public string Name { get; }

        public string DirectoryPath { get; }

        public long Length { get; }

        public DateTime LastModifiedUtc { get; }

        public bool IsDirectory => false;
    }
}
