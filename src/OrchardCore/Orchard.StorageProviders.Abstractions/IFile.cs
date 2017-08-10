using System;
using System.IO;

namespace Orchard.StorageProviders
{
    public interface IFile
    {
        string AbsolutePath { get; }
        string Path { get; }
        string Name { get; }
        string Folder { get; }
        long Length { get; }
        DateTime LastModified { get; }
        bool IsDirectory { get; }

        /// <summary>Returns the file contents as readonly stream. Caller should dispose stream when complete.</summary>
        /// <returns>The file stream</returns>
        Stream CreateReadStream();
    }
}
