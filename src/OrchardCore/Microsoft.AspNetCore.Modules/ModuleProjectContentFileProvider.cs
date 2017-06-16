using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Modules
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of Module Project Content files while in a development environment.
    /// </summary>
    public class ModuleProjectContentFileProvider : IFileProvider
    {
        private const string MappingFileFolder = "obj";
        private const string MappingFileName = "ModuleProjectContentFiles.map";

        private static Dictionary<string, string> _paths;
        private static object _synLock = new object();

        private string _contentPath;

        public ModuleProjectContentFileProvider(string rootPath, string contentPath)
        {
            _contentPath = '/' + contentPath.Replace('\\', '/').Trim('/');

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

            var path = _contentPath + subpath;

            if (_paths.ContainsKey(path))
            {
                return new PhysicalFileInfo(new FileInfo(_paths[path]));
            }

            return null;
        }

        public IChangeToken Watch(string filter)
        {
            return null;
        }
    }
}
