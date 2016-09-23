using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Orchard.FileSystem.AppData
{
    /// <summary>
    /// Abstraction of App_Data folder. All virtual paths passed in or returned are relative to "~/App_Data".
    /// Expected to work on physical filesystem, but decouples core system from web hosting apis
    /// </summary>
    public interface IAppDataFolder
    {
        string RootPath { get; }

        IFileInfo GetFileInfo(string path);
        DirectoryInfo GetDirectoryInfo(string path);

        IEnumerable<IFileInfo> ListFiles(string path);
        IEnumerable<DirectoryInfo> ListDirectories(string path);

        string Combine(params string[] paths);

        /// <summary>
        /// Creates or overwrites the file in the specified path with the specified content.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <param name="content">The content to write in the created file.</param>
        /// <remarks>If the folder doesn't exist, it will be created.</remarks>
        Task CreateFileAsync(string path, string content);

        /// <summary>
        /// Creates or overwrites the file in the specified path.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns>
        /// A <see cref="Stream"/> that provides read/write access to the file specified in path.
        /// </returns>
        /// <remarks>If the folder doesn't exist, it will be created.</remarks>
        Stream CreateFile(string path);

        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">The path and name of the file to read.</param>
        /// <returns>A string containing all lines of the file, or <code>null</code> if the file doesn't exist.</returns>
        Task<string> ReadFileAsync(string path);

        /// <summary>
        /// Open an existing file for reading.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns>
        /// A <see cref="Stream"/> that provides read access to the file specified in path.
        /// </returns>
        Stream OpenFile(string path);
        void StoreFile(string sourceFileName, string destinationPath);
        void DeleteFile(string path);

        DateTimeOffset GetFileLastWriteTimeUtc(string path);

        void CreateDirectory(string path);
        bool DirectoryExists(string path);

        string MapPath(string path);
    }
}