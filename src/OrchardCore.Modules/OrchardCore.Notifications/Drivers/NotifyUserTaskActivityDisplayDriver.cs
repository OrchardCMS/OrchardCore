using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Notifications.Activities;
using OrchardCore.Notifications.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public class NotifyUserTaskActivityDisplayDriver<TActivity, TEditViewModel> : ActivityDisplayDriver<TActivity, TEditViewModel>
    where TActivity : NotifyUserTaskActivity
    where TEditViewModel : NotifyUserTaskActivityViewModel, new()
{
    private readonly IServiceProvider _serviceProvider;

    protected virtual string EditShapeType => $"{nameof(NotifyUserTaskActivity)}_Fields_Edit";

    public NotifyUserTaskActivityDisplayDriver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override IDisplayResult Edit(TActivity model)
    {
        return Initialize<TEditViewModel>(EditShapeType, viewModel =>
        {
            return EditActivityAsync(model, viewModel);
        }).Location("Content");
    }

    public async override Task<IDisplayResult> UpdateAsync(TActivity model, IUpdateModel updater)
    {
        var viewModel = new TEditViewModel();
        if (await updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            var liquidTemplateManager = _serviceProvider.GetService<ILiquidTemplateManager>();

            if (liquidTemplateManager != null)
            {
                var S = _serviceProvider.GetRequiredService<IStringLocalizer<NotifyUserTaskActivityDisplayDriver<TActivity, TEditViewModel>>>();

                if (!liquidTemplateManager.Validate(viewModel.Subject, out var subjectErrors))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(viewModel.Subject), S["Subject field does not contain a valid Liquid expression. Details: {0}", string.Join(' ', subjectErrors)]);
                }

                if (!liquidTemplateManager.Validate(viewModel.Summary, out var summaryErrors))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(viewModel.Summary), S["Summary field does not contain a valid Liquid expression. Details: {0}", string.Join(' ', summaryErrors)]);
                }

                if (!liquidTemplateManager.Validate(viewModel.TextBody, out var textBodyErrors))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(viewModel.TextBody), S["Text Body field does not contain a valid Liquid expression. Details: {0}", string.Join(' ', textBodyErrors)]);
                }

                if (!liquidTemplateManager.Validate(viewModel.HtmlBody, out var htmlBodyErrors))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(viewModel.HtmlBody), S["HTML Body field does not contain a valid Liquid expression. Details: {0}", string.Join(' ', htmlBodyErrors)]);
                }
            }

            if (updater.ModelState.IsValid)
            {
                await UpdateActivityAsync(viewModel, model);
            }
        }

        return Edit(model);
    }

    /// <summary>
    /// Edit the view model before it's used in the editor.
    /// </summary>
    protected override ValueTask EditActivityAsync(TActivity activity, TEditViewModel model)
    {
        EditActivity(activity, model);

        return new ValueTask();
    }

    /// <summary>
    /// Edit the view model before it's used in the editor.
    /// </summary>
    protected override void EditActivity(TActivity activity, TEditViewModel model)
    {
        model.Subject = activity.Subject.Expression;
        model.Summary = activity.Summary.Expression;
        model.TextBody = activity.TextBody.Expression;
        model.HtmlBody = activity.HtmlBody.Expression;
        model.IsHtmlPreferred = activity.IsHtmlPreferred;
    }

    /// <summary>
    /// Updates the activity when the view model is validated.
    /// </summary>
    protected override Task UpdateActivityAsync(TEditViewModel model, TActivity activity)
    {
        UpdateActivity(model, activity);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the activity when the view model is validated.
    /// </summary>
    protected override void UpdateActivity(TEditViewModel model, TActivity activity)
    {
        var htmlSanitizer = _serviceProvider.GetService<IHtmlSanitizerService>();

        if (htmlSanitizer != null)
        {
            activity.Summary = new WorkflowExpression<string>(htmlSanitizer.Sanitize(model.Summary));
            activity.HtmlBody = new WorkflowExpression<string>(htmlSanitizer.Sanitize(model.HtmlBody));
        }
        else
        {
            activity.Summary = new WorkflowExpression<string>(model.Summary);
            activity.HtmlBody = new WorkflowExpression<string>(model.HtmlBody);
        }

        activity.Subject = new WorkflowExpression<string>(model.Subject);
        activity.TextBody = new WorkflowExpression<string>(model.TextBody);
        activity.IsHtmlPreferred = model.IsHtmlPreferred;
    }

    public override IDisplayResult Display(TActivity activity)
    {
        return Combine(
            Shape($"{typeof(TActivity).Name}_Fields_Thumbnail", new ActivityViewModel<TActivity>(activity))
                .Location("Thumbnail", "Content"),
            Shape($"{typeof(TActivity).Name}_Fields_Design", new ActivityViewModel<TActivity>(activity))
                .Location("Design", "Content")
        );
    }
}

public class NotifyUserTaskActivityDisplayDriver<TActivity> : NotifyUserTaskActivityDisplayDriver<TActivity, NotifyUserTaskActivityViewModel>
        where TActivity : NotifyUserTaskActivity
{
    public NotifyUserTaskActivityDisplayDriver(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}
