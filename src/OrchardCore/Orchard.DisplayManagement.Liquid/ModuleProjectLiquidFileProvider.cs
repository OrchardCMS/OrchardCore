using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace Orchard.DisplayManagement.Liquid
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of Module Project Liquid files while in a development environment.
    /// </summary>
    public class ModuleProjectLiquidFileProvider : IFileProvider
    {
        private const string MappingFileFolder = "obj";
        private const string MappingFileName = "ModuleProjectLiquidFiles.map";

        private static Dictionary<string, string> _paths;
        private static object _synLock = new object();

        public ModuleProjectLiquidFileProvider(string rootPath)
        {
            if (_paths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_paths == null)
                {
                    var path = Path.Combine(rootPath, MappingFileFolder, MappingFileName);

                    if (File.Exists(path))
                    {
                        var paths = File.ReadAllLines(path)
                            .Select(x => x.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                            .Where(x => x.Length == 2).ToDictionary(x => x[1].Replace('\\', '/'), x => x[0]);

                        _paths = new Dictionary<string, string>(paths);
                    }
                    else
                    {
                        _paths = new Dictionary<string, string>();
                    }
                }
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return null;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null)
            {
                return null;
            }

            subpath = subpath.Replace("\\", "/");

            if (_paths.ContainsKey(subpath))
            {
                return new PhysicalFileInfo(new FileInfo(_paths[subpath]));
            }

            return null;
        }

        public IChangeToken Watch(string filter)
        {
            if (filter == null)
            {
                return null;
            }

            filter = filter.Replace("\\", "/");

            if (_paths.ContainsKey(filter))
            {
                return new PollingFileChangeToken(new FileInfo(_paths[filter]));
            }

            return null;
        }
    }
}
