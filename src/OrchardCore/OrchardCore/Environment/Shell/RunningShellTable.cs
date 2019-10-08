using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Environment.Shell
{
    public class RunningShellTable : IRunningShellTable
    {
        private static readonly char[] HostSeparators = new[] { ',', ' ' };

        private ImmutableDictionary<string, ShellSettings> _shellsByHostAndPrefix = ImmutableDictionary<string, ShellSettings>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);
        private ShellSettings _default;
        private bool _hasStarMapping = false;

        public void Add(ShellSettings settings)
        {
            if (ShellHelper.DefaultShellName == settings.Name)
            {
                _default = settings;
            }

            var allHostsAndPrefix = GetAllHostsAndPrefix(settings);

            foreach (var hostAndPrefix in allHostsAndPrefix)
            {
                _hasStarMapping = _hasStarMapping || hostAndPrefix.StartsWith("*");
                _shellsByHostAndPrefix = _shellsByHostAndPrefix.SetItem(hostAndPrefix, settings);
            }
        }

        public void Remove(ShellSettings settings)
        {
            var allHostsAndPrefix = GetAllHostsAndPrefix(settings);

            _shellsByHostAndPrefix = _shellsByHostAndPrefix.RemoveRange(allHostsAndPrefix);

            if (_default == settings)
            {
                _default = null;
            }
        }

        public ShellSettings Match(HostString host, PathString path, bool fallbackToDefault = true)
        {
            // Also handles IPv6 addresses format.
            GetHostParts(host.Value, out var hostOnly, out var port);

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

        private string GetHostAndPrefix(StringSegment host, StringSegment path)
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

        private string[] GetAllHostsAndPrefix(ShellSettings shellSettings)
        {
            // For each host entry return HOST/PREFIX

            if (string.IsNullOrWhiteSpace(shellSettings.RequestUrlHost))
            {
                return new string[] { "/" + shellSettings.RequestUrlPrefix };
            }

            return shellSettings
                .RequestUrlHost
                .Split(HostSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(ruh => ruh + "/" + shellSettings.RequestUrlPrefix)
                .ToArray();
        }

        private bool DefaultIsCatchAll()
        {
            return _default != null && string.IsNullOrEmpty(_default.RequestUrlHost) && string.IsNullOrEmpty(_default.RequestUrlPrefix);
        }

        /// <summary>
        /// Parses the current value. IPv6 addresses will have brackets added if they are missing.
        /// </summary>
        /// <param name="value">The value to get the parts of.</param>
        /// <param name="host">The portion of the <paramref name="value"/> which represents the host.</param>
        /// <param name="port">The portion of the <paramref name="value"/> which represents the port.</param>
        private static void GetHostParts(StringSegment value, out StringSegment host, out StringSegment port)
        {
            int index;
            port = null;
            host = null;

            if (StringSegment.IsNullOrEmpty(value))
            {
                return;
            }
            else if ((index = value.IndexOf(']')) >= 0)
            {
                // IPv6 in brackets [::1], maybe with port
                host = value.Subsegment(0, index + 1);
                // Is there a colon and at least one character?
                if (index + 2 < value.Length && value[index + 1] == ':')
                {
                    port = value.Subsegment(index + 2);
                }
            }
            else if ((index = value.IndexOf(':')) >= 0
                && index < value.Length - 1
                && value.IndexOf(':', index + 1) >= 0)
            {
                // IPv6 without brackets ::1 is the only type of host with 2 or more colons
                host = $"[{value}]";
                port = null;
            }
            else if (index >= 0)
            {
                // Has a port
                host = value.Subsegment(0, index);
                port = value.Subsegment(index + 1);
            }
            else
            {
                host = value;
                port = null;
            }
        }
    }
}
