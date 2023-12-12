using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Indexes;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Services;
using YesSql.Sql;

namespace OrchardCore.Deployment
{
    public class Migrations : DataMigration
    {
        private readonly ISecretService _secretService;

        public Migrations(ISecretService secretService) => _secretService = secretService;

        public async Task<int> CreateAsync()
        {
            SchemaBuilder.CreateMapIndexTable<DeploymentPlanIndex>(table => table
                .Column<string>("Name")
            );

            await _secretService.GetOrCreateSecretAsync<RSASecret>(
                name: Secrets.Encryption,
                configure: secret => RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate));

            await _secretService.GetOrCreateSecretAsync<RSASecret>(
                name: Secrets.Signing,
                configure: secret => RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate));

            return 2;
        }

        public async Task<int> UpdateFrom1Async()
        {
            await _secretService.GetOrCreateSecretAsync<RSASecret>(
                name: Secrets.Encryption,
                configure: secret => RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate));

            await _secretService.GetOrCreateSecretAsync<RSASecret>(
                name: Secrets.Signing,
                configure: secret => RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate));

            return 2;
        }
    }
}
