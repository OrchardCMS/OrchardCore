using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Drivers;
using OrchardCore.DataPipelines.Ftp.ViewModels;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.DataPipelines.Ftp.Drivers;

/// <summary>
/// Renders the summary and the editor of the <see cref="UploadToFtpStep"/>, and saves its settings.
/// </summary>
public sealed class UploadToFtpStepDisplayDriver : DataPipelineStepDisplayDriver<UploadToFtpStepSettings, UploadToFtpStepViewModel>
{
    private readonly DataPipelineSecrets _secrets;
    private readonly DataPipelineEditorContext _editorContext;
    private readonly IStringLocalizer S;

    public UploadToFtpStepDisplayDriver(
        DataPipelineSecrets secrets,
        DataPipelineEditorContext editorContext,
        IStringLocalizer<UploadToFtpStepDisplayDriver> localizer)
    {
        _secrets = secrets;
        _editorContext = editorContext;
        S = localizer;
    }

    protected override string StepName => UploadToFtpStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, UploadToFtpStepSettings settings, UploadToFtpStepViewModel model)
    {
        model.Host = settings.Host;
        model.Port = settings.Port;
        model.Encryption = settings.Encryption;
        model.ValidateCertificate = settings.ValidateCertificate;
        model.Username = settings.Username;
        model.RemoteFolder = settings.RemoteFolder;
        model.Overwrite = settings.Overwrite;
        model.TimeoutSeconds = settings.TimeoutSeconds;
        model.HasPassword = !string.IsNullOrEmpty(settings.ProtectedPassword);
        model.Summary = UploadToFtpStep.GetSummary(settings);

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, UploadToFtpStepSettings settings, UploadToFtpStepViewModel model, UpdateEditorContext context)
    {
        if (model.Port is < 1 or > 65535)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Port), S["The port must be between 1 and 65535."]);
        }

        if (model.TimeoutSeconds < 1)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.TimeoutSeconds), S["The timeout must be at least one second."]);
        }

        // The editor shows other fields for another encryption, and must not show a secret that was just posted.
        _editorContext.ReloadEditor |= settings.Encryption != model.Encryption || !string.IsNullOrEmpty(model.Password) || model.RemovePassword;

        settings.Host = model.Host?.Trim();
        settings.Port = model.Port;
        settings.Encryption = model.Encryption;
        settings.ValidateCertificate = model.ValidateCertificate;
        settings.Username = model.Username?.Trim();
        settings.ProtectedPassword = _secrets.Update(settings.ProtectedPassword, model.Password, model.RemovePassword);
        settings.RemoteFolder = model.RemoteFolder?.Trim();
        settings.Overwrite = model.Overwrite;
        settings.TimeoutSeconds = model.TimeoutSeconds;

        return ValueTask.CompletedTask;
    }
}
