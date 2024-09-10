using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Contents.Workflows.Drivers;

public abstract class ContentEventDisplayDriver<TActivity, TViewModel> : ActivityDisplayDriver<TActivity, TViewModel> where TActivity : ContentEvent where TViewModel : ContentEventViewModel<TActivity>, new()
{
    protected ContentEventDisplayDriver(IContentDefinitionManager contentDefinitionManager)
    {
        ContentDefinitionManager = contentDefinitionManager;
    }

    protected IContentDefinitionManager ContentDefinitionManager { get; }

    protected override void EditActivity(TActivity source, TViewModel target)
    {
        target.SelectedContentTypeNames = source.ContentTypeFilter;
    }

    public override async Task<IDisplayResult> UpdateAsync(TActivity model, UpdateEditorContext context)
    {
        var viewModel = new TViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, x => x.SelectedContentTypeNames);

        model.ContentTypeFilter = (await FilterContentTypesQueryAsync(viewModel.SelectedContentTypeNames)).ToArray();

        return await EditAsync(model, context);
    }

    public override Task<IDisplayResult> DisplayAsync(TActivity activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Shape($"{typeof(TActivity).Name}_Fields_Thumbnail", new ContentEventViewModel<TActivity>(activity)).Location("Thumbnail", "Content"),
            Factory($"{typeof(TActivity).Name}_Fields_Design", async ctx =>
            {
                var contentTypeDefinitions = (await ContentDefinitionManager.ListTypeDefinitionsAsync()).ToDictionary(x => x.Name);
                var selectedContentTypeDefinitions = activity.ContentTypeFilter.Select(x => contentTypeDefinitions[x]).ToList();

                var shape = new ContentEventViewModel<TActivity>
                {
                    ContentTypeFilter = selectedContentTypeDefinitions,
                    Activity = activity,
                };

                return shape;
            }).Location("Design", "Content")
        );
    }

    protected async Task<IEnumerable<string>> FilterContentTypesQueryAsync(IEnumerable<string> contentTypeNames)
    {
        var contentTypeDefinitions = (await ContentDefinitionManager.ListTypeDefinitionsAsync()).ToDictionary(x => x.Name);

        return contentTypeNames.Where(x => !string.IsNullOrWhiteSpace(x) && contentTypeDefinitions.ContainsKey(x));
    }
}
