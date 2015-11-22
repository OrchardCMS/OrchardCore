using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.FileProviders;

namespace Orchard.FileSystem.AppData
{
    /// <summary>
    /// Abstraction of App_Data folder. All virtual paths passed in or returned are relative to "~/App_Data".
    /// Expected to work on physical filesystem, but decouples core system from web hosting apis
    /// </summary>
    public interface IAppDataFolder
    {
        IFileInfo GetFileInfo(string path);
        IFileInfo GetDirectoryInfo(string path);

        IEnumerable<IFileInfo> ListFiles(string path);
        IEnumerable<IFileInfo> ListDirectories(string path);

        string Combine(params string[] paths);

        void CreateFile(string path, string content);
        Stream CreateFile(string path);
        void StoreFile(string sourceFileName, string destinationPath);
        void DeleteFile(string path);

        DateTime GetFileLastWriteTimeUtc(string path);

        void CreateDirectory(string path);

        string MapPath(string path);
    }
}