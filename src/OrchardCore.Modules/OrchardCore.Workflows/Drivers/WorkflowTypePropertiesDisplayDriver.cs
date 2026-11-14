using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

/// <summary>
/// Builds the editor of the properties of a workflow type, on the pages creating and cloning a workflow type and on
/// the Workflow tab of the designer, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class WorkflowTypePropertiesDisplayDriver : DisplayDriver<WorkflowTypePropertiesViewModel>
{
    internal readonly IStringLocalizer S;

    public WorkflowTypePropertiesDisplayDriver(IStringLocalizer<WorkflowTypePropertiesDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(WorkflowTypePropertiesViewModel model, BuildEditorContext context)
    {
        // The clone page shows the locking settings in an Advanced tab, next to the Properties tab.
        var tabs = model.Page == WorkflowTypePropertiesPage.Clone;

        return Combine(
            Initialize<WorkflowTypePropertiesViewModel>("WorkflowTypeGeneral_Edit", m => Populate(model, m))
                .Location(l => InTab(l.Zone("Content", "1"), tabs, S["Properties"], "10").Card(S["General"], "10")),
            Initialize<WorkflowTypePropertiesViewModel>("WorkflowTypeExecution_Edit", m => Populate(model, m))
                .Location(l => InTab(l.Zone("Content", "2"), tabs, S["Properties"], "10").Card(S["Execution"], "20")),
            Initialize<WorkflowTypePropertiesViewModel>("WorkflowTypeHistory_Edit", m => Populate(model, m))
                .Location(l => InTab(l.Zone("Content", "3"), tabs, S["Properties"], "10").Card(S["History"], "30")),
            Initialize<WorkflowTypePropertiesViewModel>("WorkflowTypeLocking_Edit", m => Populate(model, m))
                .Location(l => InTab(l.Zone("Content", "4"), tabs, S["Advanced"], "20").Card(S["Locking"], "40")));
    }

    public override async Task<IDisplayResult> UpdateAsync(WorkflowTypePropertiesViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Name,
            m => m.IsEnabled,
            m => m.InstanceLimit,
            m => m.IsActivity,
            m => m.BranchingMode,
            m => m.FaultOnScriptErrors,
            m => m.DeleteFinishedWorkflows,
            m => m.RecordActivityData,
            m => m.LockTimeout,
            m => m.LockExpiration);

        return Edit(model, context);
    }

    // The fields keep the names the pages post, which the controllers validate and the designer reads.
    protected override void BuildPrefix(WorkflowTypePropertiesViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static PlacementLocationBuilder InTab(PlacementLocationBuilder location, bool tabs, string tab, string tabPosition)
    {
        if (!tabs)
        {
            return location;
        }

        return location.Tab(tab, tabPosition);
    }

    private static void Populate(WorkflowTypePropertiesViewModel source, WorkflowTypePropertiesViewModel model)
    {
        model.Id = source.Id;
        model.Name = source.Name;
        model.IsEnabled = source.IsEnabled;
        model.IsSingleton = source.IsSingleton;
        model.IsSingletonPerCorrelation = source.IsSingletonPerCorrelation;
        model.LockTimeout = source.LockTimeout;
        model.LockExpiration = source.LockExpiration;
        model.DeleteFinishedWorkflows = source.DeleteFinishedWorkflows;
        model.IsActivity = source.IsActivity;
        model.BranchingMode = source.BranchingMode;
        model.FaultOnScriptErrors = source.FaultOnScriptErrors;
        model.RecordActivityData = source.RecordActivityData;
        model.ReturnUrl = source.ReturnUrl;
        model.Page = source.Page;
    }
}
