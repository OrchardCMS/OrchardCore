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
            Factory($"{typeof(TActivity).Name}_Fields_Thumbnail", static (TActivity a) => new ContentTaskViewModel<TActivity>(a), activity).Location("Thumbnail", "Content"),
            Factory($"{typeof(TActivity).Name}_Fields_Design", static (TActivity a) => new ContentTaskViewModel<TActivity>(a), activity).Location("Design", "Content")
        );
    }
}
