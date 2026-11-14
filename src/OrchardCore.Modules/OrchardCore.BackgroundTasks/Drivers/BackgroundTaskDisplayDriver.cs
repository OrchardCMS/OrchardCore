using Microsoft.Extensions.Localization;
using OrchardCore.BackgroundTasks.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.BackgroundTasks.Drivers;

/// <summary>
/// Builds the editor of the settings of a background task, whose fields are grouped in tabs and cards by their placement.
/// </summary>
internal sealed class BackgroundTaskDisplayDriver : DisplayDriver<BackgroundTaskViewModel>
{
    internal readonly IStringLocalizer S;

    public BackgroundTaskDisplayDriver(IStringLocalizer<BackgroundTaskDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(BackgroundTaskViewModel model, BuildEditorContext context)
    {
        return Combine(
            Initialize<BackgroundTaskViewModel>("BackgroundTaskGeneral_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "1").Tab(S["Properties"], "10").Card(S["General"], "10")),
            Initialize<BackgroundTaskViewModel>("BackgroundTaskLocking_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "2").Tab(S["Advanced"], "20").Card(S["Locking"], "10")));
    }

    public override async Task<IDisplayResult> UpdateAsync(BackgroundTaskViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Description,
            m => m.Schedule,
            m => m.UsePipeline,
            m => m.LockTimeout,
            m => m.LockExpiration);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(BackgroundTaskViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(BackgroundTaskViewModel source, BackgroundTaskViewModel model)
    {
        model.Name = source.Name;
        model.Title = source.Title;
        model.DefaultSchedule = source.DefaultSchedule;
        model.Schedule = source.Schedule;
        model.Description = source.Description;
        model.Enable = source.Enable;
        model.LockTimeout = source.LockTimeout;
        model.LockExpiration = source.LockExpiration;
        model.UsePipeline = source.UsePipeline;
    }
}
