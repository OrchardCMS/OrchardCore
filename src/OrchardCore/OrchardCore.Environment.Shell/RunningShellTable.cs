using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OrchardCore.Environment.Shell
{
    public class RunningShellTable : IRunningShellTable
    {
        private readonly Dictionary<string, ShellSettings> _shellsByHostAndPrefix = new Dictionary<string, ShellSettings>(StringComparer.OrdinalIgnoreCase);
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private ShellSettings _default;
        private bool _hasStarMapping = false;

        public void Add(ShellSettings settings)
        {
            _lock.EnterWriteLock();
            try
            {
                if(ShellHelper.DefaultShellName == settings.Name)
                {
                    _default = settings;
                }

                var allHostsAndPrefix = GetAllHostsAndPrefix(settings);
                foreach (var hostAndPrefix in allHostsAndPrefix)
                {
                    _hasStarMapping = _hasStarMapping || hostAndPrefix.StartsWith("*");
                    _shellsByHostAndPrefix[hostAndPrefix] = settings;
                }
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
                var allHostsAndPrefix = GetAllHostsAndPrefix(settings);
                foreach (var hostAndPrefix in allHostsAndPrefix)
                {
                    _shellsByHostAndPrefix.Remove(hostAndPrefix);
                }

                if (_default == settings)
                {
                    _default = null;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public ShellSettings Match(string host, string appRelativePath, bool fallbackToDefault = true)
        {
            _lock.EnterReadLock();
            try
            {
                // Specific match?
                if(TryMatchInternal(host, appRelativePath, fallbackToDefault, out ShellSettings result))
                {
                    return result;
                }

                // Search for star mapping
                // Optimization: only if a mapping with a '*' has been added

                if (_hasStarMapping && TryMatchStarMapping(host, appRelativePath, fallbackToDefault, out result))
                {
                    return result;
                }

                // Check if the Default tenant is a catch-all
                if (fallbackToDefault && DefaultIsCatchAll())
                {
                    return _default;
                }

                // Search for another catch-all 
                if (fallbackToDefault && TryMatchInternal("", "/", fallbackToDefault, out result))
                {
                    return result;
                }

                return null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private bool TryMatchInternal(string host, string appRelativePath, bool fallbackToDefault, out ShellSettings result)
        {
            // 1. Search for Host:Port + Prefix match
            var hostAndPrefix = GetHostAndPrefix(host, appRelativePath, true);

            if (!_shellsByHostAndPrefix.TryGetValue(hostAndPrefix, out result))
            {
                // 2. Search for Host + Prefix match

                hostAndPrefix = GetHostAndPrefix(host, appRelativePath, false);

                if (!_shellsByHostAndPrefix.TryGetValue(hostAndPrefix, out result))
                {
                    // 3. Search for Host:Port only match

                    var hostAndNoPrefix = GetHostAndPrefix(host, "/", true);

                    if (!_shellsByHostAndPrefix.TryGetValue(hostAndNoPrefix, out result))
                    {
                        // 4. Search for Host only match

                        hostAndNoPrefix = GetHostAndPrefix(host, "/", false);

                        if (!_shellsByHostAndPrefix.TryGetValue(hostAndNoPrefix, out result))
                        {
                            // 5. Search for Prefix only match

                            var noHostAndPrefix = GetHostAndPrefix("", appRelativePath, false);

                            if (!_shellsByHostAndPrefix.TryGetValue(noHostAndPrefix, out result))
                            {
                                result = null;
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private bool TryMatchStarMapping(string host, string appRelativePath, bool fallbackToDefault, out ShellSettings result)
        {
            if (TryMatchInternal("*." + host, appRelativePath, fallbackToDefault, out result))
            {
                return true;
            }

            var index = -1;

            // Take the longest subdomain and look for a mapping
            while (-1 != (index = host.IndexOf('.', index + 1)))
            {
                if(TryMatchInternal("*" + host.Substring(index), appRelativePath, fallbackToDefault, out result))
                {
                    return true;
                }
            }

            result = null;
            return false;
        }

        private string GetHostAndPrefix(string host, string appRelativePath, bool includePort)
        {
            if (!includePort)
            {
                // removing the port from the host
                var hostLength = host.IndexOf(':');
                if (hostLength != -1)
                {
                    host = host.Substring(0, hostLength);
                }
            }

            // appRelativePath starts with /
            var firstSegmentIndex = appRelativePath.IndexOf('/', 1);
            if (firstSegmentIndex > -1)
            {
                return host + appRelativePath.Substring(0, firstSegmentIndex);
            }
            else
            {
                return host + appRelativePath;
            }

        }

        private string[] GetAllHostsAndPrefix(ShellSettings shellSettings)
        {
            // For each host entry return HOST/PREFIX

            if (string.IsNullOrWhiteSpace(shellSettings.RequestUrlHost))
            {
                return new string[] { "/" + shellSettings.RequestUrlPrefix };
            }

            return shellSettings
                .RequestUrlHost
                .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ruh => ruh + "/" + shellSettings.RequestUrlPrefix ?? "")
                .ToArray();
        }

        private bool DefaultIsCatchAll()
        {
            return _default != null && string.IsNullOrEmpty(_default.RequestUrlHost) && string.IsNullOrEmpty(_default.RequestUrlPrefix);
        }
    }
}