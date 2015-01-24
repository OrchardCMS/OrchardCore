using System;
using System.Collections.Generic;
using System.IO;

namespace OrchardVNext.FileSystems.AppData {
    /// <summary>
    /// Abstraction of App_Data folder. All virtual paths passed in or returned are relative to "~/App_Data". 
    /// Expected to work on physical filesystem, but decouples core system from web hosting apis
    /// </summary>
    public interface IAppDataFolder : ISingletonDependency
    {
        IEnumerable<string> ListFiles(string path);
        IEnumerable<string> ListDirectories(string path);

        string Combine(params string[] paths);

        bool FileExists(string path);
        void CreateFile(string path, string content);
        Stream CreateFile(string path);
        string ReadFile(string path);
        Stream OpenFile(string path);
        void StoreFile(string sourceFileName, string destinationPath);
        void DeleteFile(string path);

        DateTime GetFileLastWriteTimeUtc(string path);

        void CreateDirectory(string path);
        bool DirectoryExists(string path);

        string MapPath(string path);
        string GetVirtualPath(string path);
    }
}