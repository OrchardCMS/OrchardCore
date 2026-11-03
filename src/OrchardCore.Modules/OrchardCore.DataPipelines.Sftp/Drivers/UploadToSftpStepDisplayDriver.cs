using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Drivers;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Sftp.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.DataPipelines.Sftp.Drivers;

/// <summary>
/// Renders the summary and the editor of the <see cref="UploadToSftpStep"/>, and saves its settings.
/// </summary>
public sealed class UploadToSftpStepDisplayDriver : DataPipelineStepDisplayDriver<UploadToSftpStepSettings, UploadToSftpStepViewModel>
{
    private readonly DataPipelineSecrets _secrets;
    private readonly DataPipelineEditorContext _editorContext;
    private readonly IStringLocalizer S;

    public UploadToSftpStepDisplayDriver(
        DataPipelineSecrets secrets,
        DataPipelineEditorContext editorContext,
        IStringLocalizer<UploadToSftpStepDisplayDriver> localizer)
    {
        _secrets = secrets;
        _editorContext = editorContext;
        S = localizer;
    }

    protected override string StepName => UploadToSftpStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, UploadToSftpStepSettings settings, UploadToSftpStepViewModel model)
    {
        model.Host = settings.Host;
        model.Port = settings.Port;
        model.Username = settings.Username;
        model.HostKeyFingerprint = settings.HostKeyFingerprint;
        model.RemoteFolder = settings.RemoteFolder;
        model.Overwrite = settings.Overwrite;
        model.TimeoutSeconds = settings.TimeoutSeconds;
        model.HasPassword = !string.IsNullOrEmpty(settings.ProtectedPassword);
        model.HasPrivateKey = !string.IsNullOrEmpty(settings.ProtectedPrivateKey);
        model.HasPassphrase = !string.IsNullOrEmpty(settings.ProtectedPassphrase);
        model.Summary = UploadToSftpStep.GetSummary(settings);

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, UploadToSftpStepSettings settings, UploadToSftpStepViewModel model, UpdateEditorContext context)
    {
        if (model.Port is < 1 or > 65535)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Port), S["The port must be between 1 and 65535."]);
        }

        if (model.TimeoutSeconds < 1)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.TimeoutSeconds), S["The timeout must be at least one second."]);
        }

        var fingerprint = model.HostKeyFingerprint?.Trim();

        if (!string.IsNullOrEmpty(fingerprint))
        {
            var normalized = DataPipelineHostKeyFingerprint.Normalize(fingerprint);

            if (normalized is null)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.HostKeyFingerprint), S["Enter a SHA-256 fingerprint, such as SHA256:ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og."]);
            }
            else
            {
                fingerprint = "SHA256:" + normalized;
            }
        }

        _editorContext.ReloadEditor |= !string.IsNullOrEmpty(model.Password) || model.RemovePassword ||
            !string.IsNullOrEmpty(model.PrivateKey) || model.RemovePrivateKey ||
            !string.IsNullOrEmpty(model.Passphrase) || model.RemovePassphrase;

        settings.Host = model.Host?.Trim();
        settings.Port = model.Port;
        settings.Username = model.Username?.Trim();
        settings.ProtectedPassword = _secrets.Update(settings.ProtectedPassword, model.Password, model.RemovePassword);
        settings.ProtectedPrivateKey = _secrets.Update(settings.ProtectedPrivateKey, model.PrivateKey?.Trim(), model.RemovePrivateKey);

        // A passphrase without its key is useless.
        settings.ProtectedPassphrase = _secrets.Update(settings.ProtectedPassphrase, model.Passphrase, model.RemovePassphrase || model.RemovePrivateKey);
        settings.HostKeyFingerprint = string.IsNullOrEmpty(fingerprint) ? null : fingerprint;
        settings.RemoteFolder = model.RemoteFolder?.Trim();
        settings.Overwrite = model.Overwrite;
        settings.TimeoutSeconds = model.TimeoutSeconds;

        return ValueTask.CompletedTask;
    }
}
