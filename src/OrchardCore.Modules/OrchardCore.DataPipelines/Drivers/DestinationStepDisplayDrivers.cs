using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.DataPipelines.Drivers;

public sealed class SendToWebApiStepDisplayDriver : DataPipelineStepDisplayDriver<SendToWebApiStepSettings, SendToWebApiStepViewModel>
{
    private readonly DataPipelineSecrets _secrets;
    private readonly DataPipelineEditorContext _editorContext;
    private readonly IStringLocalizer S;

    public SendToWebApiStepDisplayDriver(
        DataPipelineSecrets secrets,
        DataPipelineEditorContext editorContext,
        IStringLocalizer<SendToWebApiStepDisplayDriver> localizer)
    {
        _secrets = secrets;
        _editorContext = editorContext;
        S = localizer;
    }

    protected override string StepName => SendToWebApiStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, SendToWebApiStepSettings settings, SendToWebApiStepViewModel model)
    {
        model.Url = settings.Url;
        model.Method = settings.Method;
        model.BodyMode = settings.BodyMode;
        model.FormFieldName = settings.FormFieldName;
        model.Headers = (settings.Headers ?? []).Select(header => new DataPipelineHttpHeader { Name = header.Name, Value = header.Value }).ToList();
        model.Authentication = settings.Authentication;
        model.Username = settings.Username;
        model.ApiKeyHeaderName = settings.ApiKeyHeaderName;
        model.TimeoutSeconds = settings.TimeoutSeconds;
        model.HasPassword = !string.IsNullOrEmpty(settings.ProtectedPassword);
        model.HasToken = !string.IsNullOrEmpty(settings.ProtectedToken);
        model.HasApiKey = !string.IsNullOrEmpty(settings.ProtectedApiKey);
        model.DisplayUrl = SendToWebApiStep.GetDisplayUrl(settings.Url);

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, SendToWebApiStepSettings settings, SendToWebApiStepViewModel model, UpdateEditorContext context)
    {
        var url = model.Url?.Trim();

        if (!string.IsNullOrEmpty(url) &&
            (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Url), S["Enter an absolute http or https URL, such as https://api.example.com/files."]);
        }

        var headers = (model.Headers ?? [])
            .Where(header => !string.IsNullOrWhiteSpace(header?.Name))
            .Select(header => new DataPipelineHttpHeader { Name = header.Name.Trim(), Value = header.Value?.Trim() })
            .ToList();

        foreach (var header in headers.Where(header => !SendToWebApiStep.IsValidHeaderName(header.Name)))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Headers), S["'{0}' isn't a valid header name.", header.Name]);
        }

        if (!string.IsNullOrWhiteSpace(model.ApiKeyHeaderName) && !SendToWebApiStep.IsValidHeaderName(model.ApiKeyHeaderName.Trim()))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.ApiKeyHeaderName), S["'{0}' isn't a valid header name.", model.ApiKeyHeaderName]);
        }

        if (model.TimeoutSeconds < 1)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.TimeoutSeconds), S["The timeout must be at least one second."]);
        }

        // The editor shows other fields for another body or authentication, and must not show a secret that was just
        // posted.
        _editorContext.ReloadEditor |= settings.BodyMode != model.BodyMode || settings.Authentication != model.Authentication ||
            !string.IsNullOrEmpty(model.Password) || model.RemovePassword ||
            !string.IsNullOrEmpty(model.Token) || model.RemoveToken ||
            !string.IsNullOrEmpty(model.ApiKey) || model.RemoveApiKey;

        settings.Url = url;
        settings.Method = model.Method;
        settings.BodyMode = model.BodyMode;
        settings.FormFieldName = string.IsNullOrWhiteSpace(model.FormFieldName) ? "file" : model.FormFieldName.Trim();
        settings.Headers = headers;
        settings.Authentication = model.Authentication;
        settings.Username = model.Username?.Trim();
        settings.ProtectedPassword = _secrets.Update(settings.ProtectedPassword, model.Password, model.RemovePassword);
        settings.ProtectedToken = _secrets.Update(settings.ProtectedToken, model.Token?.Trim(), model.RemoveToken);
        settings.ApiKeyHeaderName = string.IsNullOrWhiteSpace(model.ApiKeyHeaderName) ? null : model.ApiKeyHeaderName.Trim();
        settings.ProtectedApiKey = _secrets.Update(settings.ProtectedApiKey, model.ApiKey?.Trim(), model.RemoveApiKey);
        settings.TimeoutSeconds = model.TimeoutSeconds;

        return ValueTask.CompletedTask;
    }
}

public class SendToWebApiStepViewModel
{
    public string Url { get; set; }

    public DataPipelineHttpMethod Method { get; set; }

    public DataPipelineHttpBodyMode BodyMode { get; set; }

    public string FormFieldName { get; set; }

    public List<DataPipelineHttpHeader> Headers { get; set; } = [];

    public DataPipelineHttpAuthentication Authentication { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public bool RemovePassword { get; set; }

    public string Token { get; set; }

    public bool RemoveToken { get; set; }

    public string ApiKeyHeaderName { get; set; }

    public string ApiKey { get; set; }

    public bool RemoveApiKey { get; set; }

    public int TimeoutSeconds { get; set; }

    [BindNever]
    public bool HasPassword { get; set; }

    [BindNever]
    public bool HasToken { get; set; }

    [BindNever]
    public bool HasApiKey { get; set; }

    [BindNever]
    public string DisplayUrl { get; set; }
}

public sealed class SendByEmailStepDisplayDriver : DataPipelineStepDisplayDriver<SendByEmailStepSettings, SendByEmailStepViewModel>
{
    private readonly IStringLocalizer S;

    public SendByEmailStepDisplayDriver(IStringLocalizer<SendByEmailStepDisplayDriver> localizer)
    {
        S = localizer;
    }

    protected override string StepName => SendByEmailStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, SendByEmailStepSettings settings, SendByEmailStepViewModel model)
    {
        model.To = settings.To;
        model.Cc = settings.Cc;
        model.Bcc = settings.Bcc;
        model.Subject = settings.Subject;
        model.Body = settings.Body;
        model.IsHtmlBody = settings.IsHtmlBody;
        model.MaxAttachmentSizeMegabytes = settings.MaxAttachmentSizeMegabytes;
        model.SendWhenEmpty = settings.SendWhenEmpty;
        model.RecipientCount = SendByEmailStep.SplitAddresses(settings.To).Count +
            SendByEmailStep.SplitAddresses(settings.Cc).Count +
            SendByEmailStep.SplitAddresses(settings.Bcc).Count;

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, SendByEmailStepSettings settings, SendByEmailStepViewModel model, UpdateEditorContext context)
    {
        ValidateAddresses(context, nameof(model.To), model.To);
        ValidateAddresses(context, nameof(model.Cc), model.Cc);
        ValidateAddresses(context, nameof(model.Bcc), model.Bcc);

        if (model.MaxAttachmentSizeMegabytes < 1)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.MaxAttachmentSizeMegabytes), S["The maximum size of the attachments must be at least 1 MB."]);
        }

        settings.To = model.To?.Trim();
        settings.Cc = model.Cc?.Trim();
        settings.Bcc = model.Bcc?.Trim();
        settings.Subject = model.Subject?.Trim();
        settings.Body = model.Body;
        settings.IsHtmlBody = model.IsHtmlBody;
        settings.MaxAttachmentSizeMegabytes = model.MaxAttachmentSizeMegabytes;
        settings.SendWhenEmpty = model.SendWhenEmpty;

        return ValueTask.CompletedTask;
    }

    private void ValidateAddresses(UpdateEditorContext context, string property, string addresses)
    {
        var invalid = SendByEmailStep.SplitAddresses(addresses)
            .Where(address => !System.Net.Mail.MailAddress.TryCreate(address, out _))
            .ToList();

        if (invalid.Count > 0)
        {
            context.Updater.ModelState.AddModelError(Prefix, property, S["These email addresses aren't valid: {0}.", string.Join(", ", invalid)]);
        }
    }
}

public class SendByEmailStepViewModel
{
    public string To { get; set; }

    public string Cc { get; set; }

    public string Bcc { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }

    public bool IsHtmlBody { get; set; }

    public int MaxAttachmentSizeMegabytes { get; set; }

    public bool SendWhenEmpty { get; set; }

    [BindNever]
    public int RecipientCount { get; set; }
}

/// <summary>
/// The view model of the <c>DataPipelineSecretInput</c> partial, which renders a secret field of a step editor: the
/// field is always empty, and a checkbox removes the current secret.
/// </summary>
public sealed class DataPipelineSecretInputViewModel
{
    /// <summary>
    /// Gets or sets the name of the posted secret.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the label of the field.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the hint shown under the field when no secret is set.
    /// </summary>
    public string Hint { get; set; }

    /// <summary>
    /// Gets or sets the hint shown under the field when a secret is set, such as "Leave empty to keep the current
    /// password".
    /// </summary>
    public string KeepHint { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a secret is set.
    /// </summary>
    public bool HasValue { get; set; }

    /// <summary>
    /// Gets or sets the name of the posted checkbox that removes the secret.
    /// </summary>
    public string RemoveName { get; set; }

    /// <summary>
    /// Gets or sets the label of the checkbox that removes the secret.
    /// </summary>
    public string RemoveLabel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the secret spans several lines, such as a private key.
    /// </summary>
    public bool Multiline { get; set; }
}
