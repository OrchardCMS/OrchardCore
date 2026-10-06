using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Deployment;

public sealed class SecretsDeploymentStepDriver : DisplayDriver<DeploymentStep, SecretsDeploymentStep>
{
    private readonly ISecretManager _secretManager;

    public SecretsDeploymentStepDriver(ISecretManager secretManager)
    {
        _secretManager = secretManager;
    }

    public override Task<IDisplayResult> DisplayAsync(SecretsDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("SecretsDeploymentStep_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("SecretsDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
        );
    }

    public override async Task<IDisplayResult> EditAsync(SecretsDeploymentStep step, BuildEditorContext context)
    {
        var secretInfos = await _secretManager.GetSecretInfosAsync();

        // Get available encryption keys (RsaKeySecret and X509Secret)
        var encryptionKeys = secretInfos
            .Where(s => s.Type == nameof(RsaKeySecret) || s.Type == nameof(X509Secret))
            .Select(s => s.Name)
            .OrderBy(n => n)
            .ToList();

        return Initialize<SecretsDeploymentStepViewModel>("SecretsDeploymentStep_Edit", model =>
        {
            model.EncryptionKeyName = step.EncryptionKeyName;
            model.AvailableEncryptionKeys = encryptionKeys;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretsDeploymentStep step, UpdateEditorContext context)
    {
        var model = new SecretsDeploymentStepViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        step.EncryptionKeyName = model.EncryptionKeyName;

        return await EditAsync(step, context);
    }
}
