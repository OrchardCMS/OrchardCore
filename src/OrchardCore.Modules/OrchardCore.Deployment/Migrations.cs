using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Indexes;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using YesSql.Sql;

namespace OrchardCore.Deployment
{
    public class Migrations : DataMigration
    {
        private readonly ISecretService _secretService;

        public Migrations(ISecretService secretService) => _secretService = secretService;

        public async Task<int> CreateAsync()
        {
            await SchemaBuilder.CreateMapIndexTableAsync<DeploymentPlanIndex>(table => table
                .Column<string>("Name")
            );

            await _secretService.AddSecretAsync<RSASecret>(
                DeploymentSecret.Encryption,
                (secret, info) =>
                {
                    RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate);
                    info.Description = "Deployment Secret holding a raw RSA key to be used for encryption.";
                });

            await _secretService.AddSecretAsync<RSASecret>(
                DeploymentSecret.Signing,
                (secret, info) =>
                {
                    RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate);
                    info.Description = "Deployment Secret holding a raw RSA key to be used for signing.";
                });

            return 2;
        }

        public async Task<int> UpdateFrom1Async()
        {
            await _secretService.AddSecretAsync<RSASecret>(
                DeploymentSecret.Encryption,
                (secret, info) =>
                {
                    RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate);
                    info.Description = "Deployment Secret holding a raw RSA key to be used for encryption.";
                });

            await _secretService.AddSecretAsync<RSASecret>(
                DeploymentSecret.Signing,
                (secret, info) =>
                {
                    RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate);
                    info.Description = "Deployment Secret holding a raw RSA key to be used for signing.";
                });

            return 2;
        }
    }
}
