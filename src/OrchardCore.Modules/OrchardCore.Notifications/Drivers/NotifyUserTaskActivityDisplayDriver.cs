using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Notifications.Activities;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public abstract class NotifyUserTaskActivityDisplayDriver<TActivity, TEditViewModel> : ActivityDisplayDriver<TActivity, TEditViewModel>
    where TActivity : NotifyUserTaskActivity
    where TEditViewModel : class, new()
{
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly NotificationOptions _notificationOptions;

    protected readonly IStringLocalizer S;

    protected virtual string EditShapeType { get; }

    public NotifyUserTaskActivityDisplayDriver(
        IHtmlSanitizerService htmlSanitizerService,
        ILiquidTemplateManager liquidTemplateManager,
        IOptions<NotificationOptions> notificationOptions,
        IStringLocalizer stringLocalizer)
    {
        _htmlSanitizerService = htmlSanitizerService;
        _liquidTemplateManager = liquidTemplateManager;
        _notificationOptions = notificationOptions.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(TActivity activity, BuildEditorContext context)
    {
        var results = new List<IDisplayResult>();

        if (!string.IsNullOrEmpty(EditShapeType))
        {
            results.Add(Initialize<TEditViewModel>(EditShapeType, viewModel =>
            {
                return EditActivityAsync(activity, viewModel);
            }).Location("Content"));
        }

        results.Add(Initialize<NotifyUserTaskActivityViewModel>("NotifyUserTaskActivity_Fields_Edit", model =>
        {
            model.Subject = activity.Subject.Expression;
            model.Summary = activity.Summary.Expression;
            model.TextBody = activity.TextBody.Expression;
            model.HtmlBody = activity.HtmlBody.Expression;
            model.IsHtmlPreferred = activity.IsHtmlPreferred;
        }).Location("Content"));

        return Combine(results);
    }

    public override async Task<IDisplayResult> UpdateAsync(TActivity activity, UpdateEditorContext context)
    {
        var model = new NotifyUserTaskActivityViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (!_liquidTemplateManager.Validate(model.Subject, out var subjectErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Subject), S["Subject field does not contain a valid Liquid expression. Details: {0}", string.Join(' ', subjectErrors)]);
        }

        if (!_liquidTemplateManager.Validate(model.Summary, out var summaryErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Summary), S["Summary field does not contain a valid Liquid expression. Details: {0}", string.Join(' ', summaryErrors)]);
        }

        if (!_liquidTemplateManager.Validate(model.TextBody, out var textBodyErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.TextBody), S["Text Body field does not contain a valid Liquid expression. Details: {0}", string.Join(' ', textBodyErrors)]);
        }

        if (!_liquidTemplateManager.Validate(model.HtmlBody, out var htmlBodyErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.HtmlBody), S["HTML Body field does not contain a valid Liquid expression. Details: {0}", string.Join(' ', htmlBodyErrors)]);
        }

        activity.Subject = new WorkflowExpression<string>(model.Subject);
        activity.Summary = new WorkflowExpression<string>(_htmlSanitizerService.Sanitize(model.Summary ?? string.Empty));
        activity.TextBody = new WorkflowExpression<string>(model.TextBody);
        activity.HtmlBody = new WorkflowExpression<string>(_notificationOptions.DisableNotificationHtmlBodySanitizer ? model.HtmlBody : _htmlSanitizerService.Sanitize(model.HtmlBody ?? string.Empty));
        activity.IsHtmlPreferred = model.IsHtmlPreferred;

        var modelOfT = new TEditViewModel();

        await context.Updater.TryUpdateModelAsync(modelOfT, Prefix);

        await UpdateActivityAsync(modelOfT, activity);

        return Edit(activity, context);
    }

    /// <summary>
    /// Edit the view model before it's used in the editor.
    /// </summary>
    protected override ValueTask EditActivityAsync(TActivity activity, TEditViewModel model)
    {
        EditActivity(activity, model);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Edit the view model before it's used in the editor.
    /// </summary>
    protected override void EditActivity(TActivity activity, TEditViewModel model)
    {
    }

    /// <summary>
    /// Updates the activity.
    /// </summary>
    protected override Task UpdateActivityAsync(TEditViewModel model, TActivity activity)
    {
        UpdateActivity(model, activity);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the activity.
    /// </summary>
    protected override void UpdateActivity(TEditViewModel model, TActivity activity)
    {

    }

    public override Task<IDisplayResult> DisplayAsync(TActivity activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Shape($"{ActivityName}_Fields_Thumbnail", new ActivityViewModel<TActivity>(activity))
                .Location("Thumbnail", "Content"),
            Shape($"{ActivityName}_Fields_Design", new ActivityViewModel<TActivity>(activity))
                .Location("Design", "Content")
        );
    }
}

public abstract class NotifyUserTaskActivityDisplayDriver<TActivity> : NotifyUserTaskActivityDisplayDriver<TActivity, NotifyUserTaskActivityViewModel>
    where TActivity : NotifyUserTaskActivity
{
    public NotifyUserTaskActivityDisplayDriver(
        IHtmlSanitizerService htmlSanitizerService,
        ILiquidTemplateManager liquidTemplateManager,
        IOptions<NotificationOptions> notificationOptions,
        IStringLocalizer stringLocalizer)
        : base(htmlSanitizerService, liquidTemplateManager, notificationOptions, stringLocalizer)
    {
    }

    protected sealed override string EditShapeType { get; }
}
