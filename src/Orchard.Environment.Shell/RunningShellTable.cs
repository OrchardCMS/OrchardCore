using System;
using System.Collections.Generic;
using System.Threading;

namespace Orchard.Environment.Shell
{
    public class RunningShellTable : IRunningShellTable
    {
        private readonly Dictionary<string, ShellSettings> _shellsByHostAndPrefix = new Dictionary<string, ShellSettings>(StringComparer.OrdinalIgnoreCase);
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private ShellSettings _single;
        private ShellSettings _default;

        public void Add(ShellSettings settings)
        {
            _lock.EnterWriteLock();
            try
            {
                // _single is set when there is only a single tenant
                if (_single != null)
                {
                    _single = null;
                }
                else
                {
                    if (_shellsByHostAndPrefix.Count == 0)
                    {
                        _single = settings;
                    }
                }

                if(ShellHelper.DefaultShellName == settings.Name)
                {
                    _default = settings;
                }

                var hostAndPrefix = GetHostAndPrefix(settings);
                _shellsByHostAndPrefix[hostAndPrefix] = settings;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Remove(ShellSettings settings)
        {
            _lock.EnterWriteLock();
            try
            {
                var hostAndPrefix = GetHostAndPrefix(settings);
                _shellsByHostAndPrefix.Remove(hostAndPrefix);

                if (_default == settings)
                {
                    _default = null;
                }

                if (_single == settings)
                {
                    _single = null;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public ShellSettings Match(string host, string appRelativePath)
        {
            if (_single != null)
            {
                return _single;
            }

            _lock.EnterReadLock();
            try
            {
                string hostAndPrefix = GetHostAndPrefix(host, appRelativePath);

                ShellSettings result;
                if(!_shellsByHostAndPrefix.TryGetValue(hostAndPrefix, out result))
                {
                    var noHostAndPrefix = GetHostAndPrefix("", appRelativePath);

                    if (!_shellsByHostAndPrefix.TryGetValue(noHostAndPrefix, out result))
                    {
                        result = _default;

                        // no specific match, then look for star mapping
                        // TODO: implement * mapping by adding another index to match the domain with the
                        // star mapped domain. It's done by adding the star-mapped domain in the index
                        // when the settings are added to the shell table.
                    }
                }

                return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private string GetHostAndPrefix(string host, string appRelativePath)
        {
            // removing the port from the host
            var hostLength = host.IndexOf(':');
            if (hostLength != -1)
            {
                host = host.Substring(0, hostLength);
            }

            // appRelativePath starts with /
            int firstSegmentIndex = appRelativePath.IndexOf('/', 1);
            if (firstSegmentIndex > -1)
            {
                return host + appRelativePath.Substring(0, firstSegmentIndex);
            }
            else
            {
                return host + appRelativePath;
            }

        }

        private string GetHostAndPrefix(ShellSettings shellSettings)
        {
            return shellSettings.RequestUrlHost + "/" + shellSettings.RequestUrlPrefix;
        }
    }
}