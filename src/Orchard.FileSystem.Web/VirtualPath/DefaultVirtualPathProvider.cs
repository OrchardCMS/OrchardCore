using System;
using System.Collections.Generic;
using System.IO;
using Orchard.Environment;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Orchard.FileSystem.VirtualPath
{
    public class DefaultVirtualPathProvider : IVirtualPathProvider
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger _logger;

        public DefaultVirtualPathProvider(
            IHostEnvironment hostEnvironment,
            ILoggerFactory loggerFactory)
        {
            _hostEnvironment = hostEnvironment;
            _logger = loggerFactory.CreateLogger<DefaultVirtualPathProvider>();
        }

        public virtual string GetDirectoryName(string virtualPath)
        {
            return Path.GetDirectoryName(MapPath(virtualPath)).Replace(Path.DirectorySeparatorChar, '/');
        }

        public virtual IEnumerable<string> ListFiles(string path)
        {
            var adjustedPath = MapPath(path);
            if (Directory.Exists(adjustedPath))
            {
                return Directory.EnumerateFiles(adjustedPath);
            }
            return Enumerable.Empty<string>();
        }

        public virtual IEnumerable<string> ListDirectories(string path)
        {
            return Directory.EnumerateDirectories(MapPath(path));
        }

        public virtual string Combine(params string[] paths)
        {
            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
        }

        public virtual string ToAppRelative(string virtualPath)
        {
            if (IsMalformedVirtualPath(virtualPath))
                return null;

            try
            {
                string result = virtualPath;

                // In some cases, ToAppRelative doesn't normalize the path. In those cases,
                // the path is invalid.
                // Example:
                //   ApplicationPath: /Foo
                //   VirtualPath    : ~/Bar/../Blah/Blah2
                //   Result         : /Blah/Blah2  <= that is not an app relative path!
                if (!result.StartsWith("~/"))
                {
                    _logger.LogInformation("Path '{0}' cannot be made app relative: Path returned ('{1}') is not app relative.", virtualPath, result);
                    return null;
                }
                return result;
            }
            catch (Exception e)
            {
                // The initial path might have been invalid (e.g. path indicates a path outside the application root)
                _logger.LogError(string.Format("Path '{0}' cannot be made app relative", virtualPath), e);
                return null;
            }
        }

        /// <summary>
        /// We want to reject path that contains ".." going outside of the application root.
        /// ToAppRelative does that already, but we want to do the same while avoiding exceptions.
        ///
        /// Note: This method doesn't detect all cases of malformed paths, it merely checks
        ///       for *some* cases of malformed paths, so this is not a replacement for full virtual path
        ///       verification through VirtualPathUtilty methods.
        ///       In other words, !IsMalformed does *not* imply "IsWellformed".
        /// </summary>
        public bool IsMalformedVirtualPath(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
                return true;

            if (virtualPath.IndexOf("..") >= 0)
            {
                virtualPath = virtualPath.Replace(Path.DirectorySeparatorChar, '/');
                string rootPrefix = virtualPath.StartsWith("~/") ? "~/" : virtualPath.StartsWith("/") ? "/" : "";
                if (!string.IsNullOrEmpty(rootPrefix))
                {
                    string[] terms = virtualPath.Substring(rootPrefix.Length).Split('/');
                    int depth = 0;
                    foreach (var term in terms)
                    {
                        if (term == "..")
                        {
                            if (depth == 0)
                            {
                                _logger.LogInformation("Path '{0}' cannot be made app relative: Too many '..'", virtualPath);
                                return true;
                            }
                            depth--;
                        }
                        else
                        {
                            depth++;
                        }
                    }
                }
            }

            return false;
        }

        public virtual Stream OpenFile(string virtualPath)
        {
            return File.Open(MapPath(virtualPath), FileMode.Open);
        }

        public virtual string ReadFile(string virtualPath)
        {
            return File.ReadAllText(MapPath(virtualPath));
        }

        public virtual StreamWriter CreateText(string virtualPath)
        {
            return File.CreateText(MapPath(virtualPath));
        }

        public virtual Stream CreateFile(string virtualPath)
        {
            return File.Create(MapPath(virtualPath));
        }

        public virtual DateTime GetFileLastWriteTimeUtc(string virtualPath)
        {
            return File.GetLastWriteTime(MapPath(virtualPath)).ToUniversalTime();
        }

        public string GetFileHash(string virtualPath)
        {
            return GetFileHash(virtualPath, new[] { virtualPath });
        }

        public string GetFileHash(string virtualPath, IEnumerable<string> dependencies)
        {
            throw new NotImplementedException("TODO");
        }

        public virtual void DeleteFile(string virtualPath)
        {
            File.Delete(MapPath(virtualPath));
        }

        public virtual string MapPath(string virtualPath)
        {
            if (!virtualPath.StartsWith("~", StringComparison.OrdinalIgnoreCase))
                return virtualPath;

            return _hostEnvironment.MapPath(virtualPath);
        }

        public virtual bool FileExists(string virtualPath)
        {
            return File.Exists(MapPath(virtualPath));
        }

        public virtual bool TryFileExists(string virtualPath)
        {
            if (IsMalformedVirtualPath(virtualPath))
                return false;

            try
            {
                return FileExists(virtualPath);
            }
            catch (Exception e)
            {
                _logger.LogError(string.Format("File '{0}' can not be checked for existence. Assuming doesn't exist.", virtualPath), e);
                return false;
            }
        }

        public virtual bool DirectoryExists(string virtualPath)
        {
            return Directory.Exists(MapPath(virtualPath));
        }

        public virtual void CreateDirectory(string virtualPath)
        {
            Directory.CreateDirectory(MapPath(virtualPath));
        }

        public virtual void DeleteDirectory(string virtualPath)
        {
            Directory.Delete(MapPath(virtualPath));
        }
    }
}