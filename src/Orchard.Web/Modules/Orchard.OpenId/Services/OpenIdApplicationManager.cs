using Orchard.OpenId.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenIddict;
using YesSql.Core.Services;
using Orchard.OpenId.Indexes;

namespace Orchard.OpenId.Services
{
    public class OpenIdApplicationManager : OpenIddict.OpenIddictApplicationManager<OpenIdApplication>, IOpenIdApplicationManager
    {
        private readonly ISession _session;

        public OpenIdApplicationManager(IServiceProvider services, IOpenIddictApplicationStore<OpenIdApplication> store, ILogger<OpenIddictApplicationManager<OpenIdApplication>> logger, ISession session) : base(services, store, logger)
        {
            _session = session;
        }

        public virtual Task<IEnumerable<OpenIdApplication>> GetAppsAsync(int skip, int pageSize)
        {
            return _session.QueryAsync<OpenIdApplication, OpenIdApplicationIndex>().Skip(skip).Take(pageSize).List();
        }

        public virtual Task<int> GetCount()
        {
            return _session.QueryAsync<OpenIdApplication, OpenIdApplicationIndex>().Count();
        }
    }
}
