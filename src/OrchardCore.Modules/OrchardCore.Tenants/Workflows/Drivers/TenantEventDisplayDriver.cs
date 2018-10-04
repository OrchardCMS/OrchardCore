using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Tenants.Workflows.Drivers
{
    public abstract class TenantEventDisplayDriver<TActivity, TViewModel> : ActivityDisplayDriver<TActivity, TViewModel> 
        where TActivity : TenantEvent where TViewModel : TenantEventViewModel<TActivity>, new()
    {
        protected TenantEventDisplayDriver(IShellSettingsManager shellSettingsManager)
        {
            ShellSettingsManager = shellSettingsManager;
        }

        protected IShellSettingsManager ShellSettingsManager { get; }

        protected override void EditActivity(TActivity source, TViewModel target)
        {
            //target.SelectedContentTypeNames = source.ContentTypeFilter;
        }

        public async override Task<IDisplayResult> UpdateAsync(TActivity model, IUpdateModel updater)
        {
            var viewModel = new TViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix, x => x.SelectedContentTypeNames))
            {
                //model.ContentTypeFilter = FilterContentTypesQuery(viewModel.SelectedContentTypeNames).ToList();
            }
            return Edit(model);
        }

        public override IDisplayResult Display(TActivity activity)
        {
            return Combine(
                Shape($"{typeof(TActivity).Name}_Fields_Thumbnail", new TenantEventViewModel<TActivity>(activity)).Location("Thumbnail", "Content"),
                Factory($"{typeof(TActivity).Name}_Fields_Design", ctx =>
                {
                    //var contentTypeDefinitions = ContentDefinitionManager.ListTypeDefinitions().ToDictionary(x => x.Name);
                    //var selectedContentTypeDefinitions = activity.ContentTypeFilter.Select(x => contentTypeDefinitions[x]).ToList();

                    var shape = new TenantEventViewModel<TActivity>();
                    //shape.ContentTypeFilter = selectedContentTypeDefinitions;
                    shape.Activity = activity;

                    return shape;
                    
                }).Location("Design", "Content")
            );
        }
    }
}
