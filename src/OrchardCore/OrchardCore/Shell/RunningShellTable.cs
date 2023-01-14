using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Environment.Shell
{
    public class RunningShellTable : IRunningShellTable
    {
        private ImmutableDictionary<string, ShellSettings> _shellsByHostAndPrefix = ImmutableDictionary<string, ShellSettings>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);
        private ShellSettings _default;
        private bool _hasStarMapping = false;

        public void Add(ShellSettings settings)
        {
            if (settings.IsDefaultShell())
            {
                _default = settings;
            }

            var allHostsAndPrefix = GetAllHostsAndPrefix(settings);

            var settingsByHostAndPrefix = new Dictionary<string, ShellSettings>();

            foreach (var hostAndPrefix in allHostsAndPrefix)
            {
                _hasStarMapping = _hasStarMapping || hostAndPrefix.StartsWith('*');
                settingsByHostAndPrefix.TryAdd(hostAndPrefix, settings);
            }

            lock (this)
            {
                _shellsByHostAndPrefix = _shellsByHostAndPrefix.SetItems(settingsByHostAndPrefix);
            }
        }

        public void Remove(ShellSettings settings)
        {
            var allHostsAndPrefix = _shellsByHostAndPrefix
                .Where(kv => kv.Value.Name == settings.Name)
                .Select(kv => kv.Key)
                .ToArray();

            lock (this)
            {
                _shellsByHostAndPrefix = _shellsByHostAndPrefix.RemoveRange(allHostsAndPrefix);
            }

            if (_default == settings)
            {
                _default = null;
            }
        }

        public ShellSettings Match(HostString host, PathString path, bool fallbackToDefault = true)
        {
            // Supports IPv6 format.
            var hostOnly = host.Host;

            // Specific match?
            if (TryMatchInternal(host.Value, hostOnly, path.Value, out var result))
            {
                return result;
            }

            // Search for star mapping
            // Optimization: only if a mapping with a '*' has been added

            if (_hasStarMapping && TryMatchStarMapping(host.Value, hostOnly, path.Value, out result))
            {
                return result;
            }

            // Check if the Default tenant is a catch-all
            if (fallbackToDefault && DefaultIsCatchAll())
            {
                return _default;
            }

            // Search for another catch-all
            if (fallbackToDefault && TryMatchInternal("", "", "/", out result))
            {
                return result;
            }

            return null;
        }

        private bool TryMatchInternal(StringSegment host, StringSegment hostOnly, StringSegment path, out ShellSettings result)
        {
            // 1. Search for Host:Port + Prefix match

            if (host.Length == 0 || !_shellsByHostAndPrefix.TryGetValue(GetHostAndPrefix(host, path), out result))
            {
                // 2. Search for Host + Prefix match

                if (host.Length == hostOnly.Length || !_shellsByHostAndPrefix.TryGetValue(GetHostAndPrefix(hostOnly, path), out result))
                {
                    // 3. Search for Host:Port only match

                    if (host.Length == 0 || !_shellsByHostAndPrefix.TryGetValue(GetHostAndPrefix(host, "/"), out result))
                    {
                        // 4. Search for Host only match

                        if (host.Length == hostOnly.Length || !_shellsByHostAndPrefix.TryGetValue(GetHostAndPrefix(hostOnly, "/"), out result))
                        {
                            // 5. Search for Prefix only match

                            if (!_shellsByHostAndPrefix.TryGetValue(GetHostAndPrefix("", path), out result))
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

        private bool TryMatchStarMapping(StringSegment host, StringSegment hostOnly, StringSegment path, out ShellSettings result)
        {
            if (TryMatchInternal("*." + host, "*." + hostOnly, path, out result))
            {
                return true;
            }

            var index = -1;

            // Take the longest subdomain and look for a mapping
            while (-1 != (index = host.IndexOf('.', index + 1)))
            {
                if (TryMatchInternal("*" + host.Subsegment(index), "*" + hostOnly.Subsegment(index), path, out result))
                {
                    return true;
                }
            }

            result = null;
            return false;
        }

        private static string GetHostAndPrefix(StringSegment host, StringSegment path)
        {
            // The request path starts with a leading '/'
            var firstSegmentIndex = path.Length > 0 ? path.IndexOf('/', 1) : -1;
            if (firstSegmentIndex > -1)
            {
                return host + path.Subsegment(0, firstSegmentIndex).Value;
            }
            else
            {
                return host + path.Value;
            }
        }

        private static string[] GetAllHostsAndPrefix(ShellSettings shellSettings)
        {
            // For each host entry return HOST/PREFIX

            if (String.IsNullOrWhiteSpace(shellSettings.RequestUrlHost))
            {
                return new string[] { "/" + shellSettings.RequestUrlPrefix };
            }

            return shellSettings
                .RequestUrlHosts
                .Select(ruh => ruh + "/" + shellSettings.RequestUrlPrefix)
                .ToArray();
        }

        private bool DefaultIsCatchAll()
        {
            return _default != null && String.IsNullOrEmpty(_default.RequestUrlHost) && String.IsNullOrEmpty(_default.RequestUrlPrefix);
        }
    }
}
