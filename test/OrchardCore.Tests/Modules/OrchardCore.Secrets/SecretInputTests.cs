using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Secrets;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SecretInputTests
{
    private const string Prefix = "Settings.Credential";

    private readonly IDataProtector _protector = new EphemeralDataProtectionProvider().CreateProtector("Tests");
    private readonly Mock<ISecretManager> _secretManager = new();
    private readonly ModelStateDictionary _modelState = new();

    public SecretInputTests()
    {
        _secretManager.Setup(m => m.GetSecretInfosAsync()).ReturnsAsync(
            [new SecretInfo { Name = "Existing", Store = "Database", Type = typeof(TextSecret).FullName }]);
    }

    [Fact]
    public void Create_WithSecretName_SelectsSecretSource()
    {
        var model = SecretInputViewModel.Create("protected", "Existing");

        Assert.Equal(SecretInputSource.Secret, model.Source);
        Assert.Equal("Existing", model.SecretName);
        Assert.True(model.HasValue);
    }

    [Fact]
    public async Task UpdateAsync_WithoutSecretsFeature_ProtectsTheValue()
    {
        var context = CreateContext(withSecrets: false);

        var result = await new SecretInputViewModel { Value = "clear" }.UpdateAsync(context);

        Assert.True(result.Succeeded);
        Assert.Null(result.SecretName);
        Assert.Equal("clear", _protector.Unprotect(result.ProtectedValue));
    }

    [Fact]
    public async Task UpdateAsync_WithoutSecretsFeatureAndNoValue_KeepsTheStoredValues()
    {
        var context = CreateContext(withSecrets: false);
        context.ProtectedValue = "protected";
        context.SecretName = "Existing";

        // A posted source is ignored when the Secrets feature is disabled.
        var result = await new SecretInputViewModel { Source = SecretInputSource.Secret, SecretName = "Other" }.UpdateAsync(context);

        Assert.True(result.Succeeded);
        Assert.Equal("protected", result.ProtectedValue);
        Assert.Equal("Existing", result.SecretName);
    }

    [Fact]
    public async Task UpdateAsync_ReferencingASecret_ClearsTheProtectedValue()
    {
        var context = CreateContext();
        context.ProtectedValue = "protected";

        var result = await new SecretInputViewModel { Source = SecretInputSource.Secret, SecretName = "Existing" }.UpdateAsync(context);

        Assert.True(result.Succeeded);
        Assert.Null(result.ProtectedValue);
        Assert.Equal("Existing", result.SecretName);
    }

    [Fact]
    public async Task UpdateAsync_ReferencingAMissingSecret_Fails()
    {
        var context = CreateContext();

        var result = await new SecretInputViewModel { Source = SecretInputSource.Secret, SecretName = "Missing" }.UpdateAsync(context);

        Assert.False(result.Succeeded);
        Assert.True(_modelState.ContainsKey($"{Prefix}.{nameof(SecretInputViewModel.SecretName)}"));
    }

    [Fact]
    public async Task UpdateAsync_SwitchingFromSecretToValue_ClearsTheSecretName()
    {
        var context = CreateContext();
        context.SecretName = "Existing";

        var result = await new SecretInputViewModel { Source = SecretInputSource.Value, Value = "clear" }.UpdateAsync(context);

        Assert.True(result.Succeeded);
        Assert.Null(result.SecretName);
        Assert.Equal("clear", _protector.Unprotect(result.ProtectedValue));
    }

    [Fact]
    public async Task UpdateAsync_SwitchingFromSecretToEmptyValue_Fails()
    {
        var context = CreateContext();
        context.SecretName = "Existing";

        var result = await new SecretInputViewModel { Source = SecretInputSource.Value }.UpdateAsync(context);

        Assert.False(result.Succeeded);
        Assert.True(_modelState.ContainsKey($"{Prefix}.{nameof(SecretInputViewModel.Value)}"));
    }

    [Fact]
    public async Task GetSecretValueAsync_PrefersTheReferencedSecret()
    {
        _secretManager.Setup(m => m.GetSecretAsync<TextSecret>("Existing")).ReturnsAsync(new TextSecret { Text = "from-secret" });

        var value = await CreateServices(withSecrets: true).GetSecretValueAsync("Existing", _protector.Protect("from-settings"), _protector);

        Assert.Equal("from-secret", value);
    }

    [Fact]
    public async Task GetSecretValueAsync_WithoutSecretsFeature_UsesTheProtectedValue()
    {
        var value = await CreateServices(withSecrets: false).GetSecretValueAsync("Existing", _protector.Protect("from-settings"), _protector);

        Assert.Equal("from-settings", value);
    }

    [Fact]
    public async Task GetSecretValueAsync_WithUndecryptableValue_ReturnsNull()
    {
        var value = await CreateServices(withSecrets: false).GetSecretValueAsync(null, "not-protected", _protector, NullLogger.Instance);

        Assert.Null(value);
    }

    [Fact]
    public async Task MoveToSecretAsync_SavesInTheSelectedStore()
    {
        var migration = new SecretMigration { Store = "Vault" };
        string referenced = null;

        var moved = await migration.MoveToSecretAsync(_secretManager.Object, _protector, _protector.Protect("stored"), "New", "Test: Credential",
            secretName =>
            {
                referenced = secretName;

                return Task.CompletedTask;
            });

        Assert.True(moved);
        Assert.True(Assert.Single(migration.Results).Succeeded);
        _secretManager.Verify(m => m.SaveSecretAsync("New", It.Is<TextSecret>(s => s.Text == "stored"), "Vault", It.IsAny<SecretSaveOptions>()), Times.Once);

        // The settings reference the secret only once every secret is saved.
        Assert.Null(referenced);
        await migration.UpdateSettingsAsync();
        Assert.Equal("New", referenced);
    }

    [Fact]
    public async Task MoveToSecretAsync_ReportsFailuresWithoutValues()
    {
        var migration = new SecretMigration { Store = "Vault" };
        var failing = new Mock<IDataProtector>();
        failing.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Throws(new CryptographicException("Key missing"));

        var referenced = false;
        Task Reference(string secretName)
        {
            referenced = true;

            return Task.CompletedTask;
        }

        Assert.False(await migration.MoveToSecretAsync(_secretManager.Object, _protector, "x", "Existing", "Exists", Reference));
        Assert.False(await migration.MoveToSecretAsync(_secretManager.Object, failing.Object, "x", "New", "Undecryptable", Reference));
        Assert.False(await migration.MoveToSecretAsync(_secretManager.Object, _protector, "x", " ", "Unnamed", Reference));
        await migration.UpdateSettingsAsync();
        Assert.False(referenced);

        Assert.Equal(
            new[] { SecretMigrationError.AlreadyExists, SecretMigrationError.DecryptionFailed, SecretMigrationError.MissingName },
            migration.Results.Select(result => result.Error));
        _secretManager.Verify(m => m.SaveSecretAsync(It.IsAny<string>(), It.IsAny<TextSecret>(), It.IsAny<string>(), It.IsAny<SecretSaveOptions>()), Times.Never);
    }

    [Fact]
    public async Task MoveToSecretAsync_WhenTheStoreFails_ReportsStoreFailed()
    {
        _secretManager.Setup(m => m.SaveSecretAsync(It.IsAny<string>(), It.IsAny<TextSecret>(), It.IsAny<string>(), It.IsAny<SecretSaveOptions>()))
            .ThrowsAsync(new InvalidOperationException("Store unavailable"));
        var migration = new SecretMigration { Store = "Vault" };

        Assert.False(await migration.MoveToSecretAsync(_secretManager.Object, _protector, _protector.Protect("stored"), "New", "Test", _ => Task.CompletedTask));
        Assert.Equal(SecretMigrationError.StoreFailed, Assert.Single(migration.Results).Error);
    }

    private SecretInputUpdateContext CreateContext(bool withSecrets = true)
        => new(CreateServices(withSecrets), _protector, _modelState, Prefix);

    private ServiceProvider CreateServices(bool withSecrets)
    {
        var services = new ServiceCollection().AddLogging().AddLocalization();

        if (withSecrets)
        {
            services.AddSingleton(_secretManager.Object);
        }

        return services.BuildServiceProvider();
    }
}
