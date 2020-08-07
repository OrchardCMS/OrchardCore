using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.Data;
using OrchardCore.Environment.Cache;
using OrchardCore.Secrets.Models;
using YesSql;

namespace OrchardCore.Secrets.Services
{
    public class AuthorizationSecretService : ISecretService<AuthorizationSecret>
    {
        private readonly ISecretCoordinator _secretCoordinator;

        public AuthorizationSecretService(ISecretCoordinator secretCoordinator)
        {
            _secretCoordinator = secretCoordinator;
        }

        public Task<AuthorizationSecret> GetSecretAsync(string key) => _secretCoordinator.GetSecretAsync<AuthorizationSecret>(key);
    }
}
