using System;
using System.Collections.Generic;
using System.Threading;

namespace OrchardCore.Tenant
{
    public class RunningTenantTable : IRunningTenantTable
    {
        private readonly Dictionary<string, TenantSettings> _tenantsByHostAndPrefix = new Dictionary<string, TenantSettings>(StringComparer.OrdinalIgnoreCase);
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private TenantSettings _single;
        private TenantSettings _default;

        public void Add(TenantSettings settings)
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
                    if (_tenantsByHostAndPrefix.Count == 0)
                    {
                        _single = settings;
                    }
                }

                if(TenantHelper.DefaultTenantName == settings.Name)
                {
                    _default = settings;
                }

                var hostAndPrefix = GetHostAndPrefix(settings);
                _tenantsByHostAndPrefix[hostAndPrefix] = settings;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Remove(TenantSettings settings)
        {
            _lock.EnterWriteLock();
            try
            {
                var hostAndPrefix = GetHostAndPrefix(settings);
                _tenantsByHostAndPrefix.Remove(hostAndPrefix);

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

        public TenantSettings Match(string host, string appRelativePath)
        {
            if (_single != null)
            {
                return _single;
            }

            _lock.EnterReadLock();
            try
            {
                string hostAndPrefix = GetHostAndPrefix(host, appRelativePath);

                TenantSettings result;
                if(!_tenantsByHostAndPrefix.TryGetValue(hostAndPrefix, out result))
                {
                    var noHostAndPrefix = GetHostAndPrefix("", appRelativePath);

                    if (!_tenantsByHostAndPrefix.TryGetValue(noHostAndPrefix, out result))
                    {
                        result = _default;

                        // no specific match, then look for star mapping
                        // TODO: implement * mapping by adding another index to match the domain with the
                        // star mapped domain. It's done by adding the star-mapped domain in the index
                        // when the settings are added to the tenant table.
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

        private string GetHostAndPrefix(TenantSettings tenantSettings)
        {
            return tenantSettings.RequestUrlHost + "/" + tenantSettings.RequestUrlPrefix;
        }
    }
}