using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DataOrchestrator.Activities;

namespace OrchardCore.DataOrchestrator.ViewModels;

/// <summary>
/// View model for rendering ETL activity shapes.
/// </summary>
public class EtlActivityViewModel<TActivity> : ShapeViewModel
    where TActivity : IEtlActivity
{
    public EtlActivityViewModel()
    {
    }

    public EtlActivityViewModel(TActivity activity)
    {
        Activity = activity;
    }

    [BindNever]
    public TActivity Activity { get; set; }
}
