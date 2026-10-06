using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Deployment;
using OrchardCore.Recipes.Models;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Deployment;
using OrchardCore.Secrets.Recipes;
using OrchardCore.Secrets.Services;
using ISecret = OrchardCore.Secrets.ISecret;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SecretDeploymentTests
{
    [Fact]
    public async Task ExportAndImport_RoundTripCustomTypeAndMetadataFromSpecificStore()
    {
        var (manager, source, recipe) = CreateServices();
        var result = new DeploymentPlanResult(null, new RecipeDescriptor());
        await source.ProcessDeploymentStepAsync(new SecretsDeploymentStep { EncryptionKeyName = "key" }, result);
        var step = JsonNode.Parse(Assert.Single(result.Steps).ToJsonString()).AsObject();
        var context = new RecipeExecutionContext { Name = "Secrets", Step = step };

        await recipe.ExecuteAsync(context);

        Assert.Empty(context.Errors);
        manager.Verify(m => m.GetSecretAsync<ISecret>("custom", "AzureKeyVault"), Times.Once);
        manager.Verify(m => m.GetSecretAsync<ISecret>("custom"), Times.Never);
        manager.Verify(m => m.SaveSecretAsync("custom",
            It.Is<ISecret>(s => s is CustomSecret && ((CustomSecret)s).Value == "custom value"),
            "AzureKeyVault", It.Is<SecretSaveOptions>(o => o.Description == "description" && o.ExpiresUtc == Expiration)), Times.Once);
    }

    [Theory]
    [InlineData("Store")]
    [InlineData("Type")]
    [InlineData("Description")]
    [InlineData("ExpiresUtc")]
    [InlineData("EncryptionKeyName")]
    [InlineData("EncryptedData")]
    public async Task Import_DoesNotSaveTamperedEncryptedRecipe(string field)
    {
        var (manager, source, recipe) = CreateServices();
        var result = new DeploymentPlanResult(null, new RecipeDescriptor());
        await source.ProcessDeploymentStepAsync(new SecretsDeploymentStep { EncryptionKeyName = "key" }, result);
        var step = Assert.Single(result.Steps);
        if (field == "EncryptionKeyName")
        {
            step.Remove(field);
        }
        else if (field == "EncryptedData")
        {
            step["Secrets"]["custom"].AsObject().Remove(field);
        }
        else
        {
            step["Secrets"]["custom"]["SecretInfo"][field] = field == "ExpiresUtc" ? "2031-01-01T00:00:00Z" : "tampered";
        }

        var context = new RecipeExecutionContext { Name = "Secrets", Step = step };
        await recipe.ExecuteAsync(context);

        Assert.NotEmpty(context.Errors);
        manager.Verify(m => m.SaveSecretAsync(It.IsAny<string>(), It.IsAny<ISecret>(), It.IsAny<string>(), It.IsAny<SecretSaveOptions>()), Times.Never);
    }

    [Fact]
    public async Task Export_ReadFailureDoesNotProduceMetadataOnlyFallback()
    {
        var (manager, source, _) = CreateServices();
        manager.Setup(m => m.GetSecretAsync<ISecret>("custom", "AzureKeyVault")).ReturnsAsync((ISecret)null);
        var result = new DeploymentPlanResult(null, new RecipeDescriptor());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            source.ProcessDeploymentStepAsync(new SecretsDeploymentStep { EncryptionKeyName = "key" }, result));
        Assert.Empty(result.Steps);
    }

    [Fact]
    public async Task Export_DuplicateNamesAcrossStoresDoNotOverwriteEachOther()
    {
        var (manager, source, _) = CreateServices();
        manager.Setup(m => m.GetSecretInfosAsync()).ReturnsAsync(
            [
                new SecretInfo { Name = "duplicate", Store = "Database", Type = nameof(TextSecret) },
                new SecretInfo { Name = "duplicate", Store = "AzureKeyVault", Type = nameof(TextSecret) },
            ]);
        var result = new DeploymentPlanResult(null, new RecipeDescriptor());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            source.ProcessDeploymentStepAsync(new SecretsDeploymentStep(), result));

        Assert.Empty(result.Steps);
    }

    private static readonly DateTime Expiration = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static (Mock<ISecretManager> Manager, SecretsDeploymentSource Source, SecretsRecipeStep Recipe) CreateServices()
    {
        using var rsa = RSA.Create(2048);
        var manager = new Mock<ISecretManager>();
        manager.Setup(m => m.GetSecretAsync<RsaKeySecret>("key")).ReturnsAsync(new RsaKeySecret
        {
            PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey()),
            PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey()),
            IncludesPrivateKey = true,
        });
        manager.Setup(m => m.GetSecretInfosAsync()).ReturnsAsync(
            [new SecretInfo
            {
                Name = "custom",
                Store = "AzureKeyVault",
                Type = typeof(CustomSecret).FullName,
                Description = "description",
                ExpiresUtc = Expiration,
            }]);
        manager.Setup(m => m.GetSecretAsync<ISecret>("custom", "AzureKeyVault")).ReturnsAsync(new CustomSecret { Value = "custom value" });
        var encryption = new SecretEncryptionService(manager.Object, [new CustomSecretProvider()]);
        var source = new SecretsDeploymentSource(manager.Object, encryption, NullLogger<SecretsDeploymentSource>.Instance);
        var localizer = new Mock<IStringLocalizer<SecretsRecipeStep>>();
        localizer.Setup(s => s[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, string.Format(key, args)));
        var recipe = new SecretsRecipeStep(manager.Object, encryption, new ConfigurationBuilder().Build(),
            NullLogger<SecretsRecipeStep>.Instance, localizer.Object);
        return (manager, source, recipe);
    }

    public sealed class CustomSecret : ISecret
    {
        public string Value { get; set; }
    }

    private sealed class CustomSecretProvider : SecretTypeProvider<CustomSecret>
    {
        public override string DisplayName => "Custom";
        public override string Description => "Custom secret for deployment coverage.";
    }
}
