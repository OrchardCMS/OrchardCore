using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Contents.Workflows.Drivers;

public abstract class ContentTaskDisplayDriver<TActivity, TViewModel> : ActivityDisplayDriver<TActivity, TViewModel> where TActivity : ContentTask where TViewModel : ContentTaskViewModel<TActivity>, new()
{
    public override Task<IDisplayResult> DisplayAsync(TActivity activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Shape($"{typeof(TActivity).Name}_Fields_Thumbnail", new ContentTaskViewModel<TActivity>(activity)).Location("Thumbnail", "Content"),
            Shape($"{typeof(TActivity).Name}_Fields_Design", new ContentTaskViewModel<TActivity>(activity)).Location("Design", "Content")
        );
    }
}
