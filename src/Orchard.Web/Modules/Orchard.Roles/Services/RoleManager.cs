using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Orchard.Environment.Cache.Abstractions;
using Orchard.Roles.Models;
using YesSql.Core.Services;

namespace Orchard.Roles.Services
{
    public class RoleManager : IRoleManager
    {
        private const string Key = "RolesManager.Roles";

        private readonly ISession _session;
        private RolesDocument _roles;
        private readonly ISignal _signal;
        private readonly IMemoryCache _memoryCache;

        public RoleManager(ISession session, IMemoryCache memoryCache, ISignal signal)
        {
            _memoryCache = memoryCache;
            _signal = signal;
            _session = session;
        }

        public async Task<RolesDocument> GetRolesAsync()
        {
            return await _memoryCache.GetOrCreateAsync(Key, async (entry) =>
            {
                _roles = await _session.QueryAsync<RolesDocument>().FirstOrDefault();

                if (_roles == null)
                {
                    _roles = new RolesDocument();
                    UpdateRoles();
                }

                entry.ExpirationTokens.Add(_signal.GetToken(Key));

                return _roles;
            });
        }

        public void UpdateRoles()
        {
            if (_roles != null)
            {
                _roles.Serial++;
                _session.Save(_roles);
                _signal.SignalToken(Key);
            }
        }
    }
}
