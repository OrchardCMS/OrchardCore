using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Display
{
    /// <summary>
    /// Base class for activity drivers.
    /// </summary>
    public abstract class ActivityDisplayDriver<TActivity> : DisplayDriver<IActivity, TActivity> where TActivity : class, IActivity
    {

    }

    /// <summary>
    /// Base class for activity drivers using a strongly typed view model.
    /// </summary>
    public abstract class ActivityDisplayDriver<TActivity, TEditViewModel> : ActivityDisplayDriver<TActivity> where TActivity : class, IActivity where TEditViewModel : class, new()
    {
        private static string ThumbnailshapeType = $"{typeof(TActivity).Name}_Fields_Thumbnail";
        private static string DesignShapeType = $"{typeof(TActivity).Name}_Fields_Design";
        private static string EditShapeType = $"{typeof(TActivity).Name}_Fields_Edit";

        public override IDisplayResult Display(TActivity activity)
        {
            return Combine(
                Shape(ThumbnailshapeType, new ActivityViewModel<TActivity>(activity)).Location("Thumbnail", "Content"),
                Shape(DesignShapeType, new ActivityViewModel<TActivity>(activity)).Location("Design", "Content")
            );
        }

        public override IDisplayResult Edit(TActivity activity)
        {
            return Initialize<TEditViewModel>(EditShapeType, model =>
            {
                return EditActivityAsync(activity, model);
            }).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(TActivity activity, IUpdateModel updater)
        {
            var viewModel = new TEditViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                await UpdateActivityAsync(viewModel, activity);
            }

            return Edit(activity);
        }

        /// <summary>
        /// Edit the view model before it's used in the editor.
        /// </summary>
        protected virtual Task EditActivityAsync(TActivity activity, TEditViewModel model)
        {
            EditActivity(activity, model);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Edit the view model before it's used in the editor.
        /// </summary>
        protected virtual void EditActivity(TActivity activity, TEditViewModel model)
        {
        }

        /// <summary>
        /// Updates the activity when the view model is validated.
        /// </summary>
        protected virtual Task UpdateActivityAsync(TEditViewModel model, TActivity activity)
        {
            UpdateActivity(model, activity);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates the activity when the view model is validated.
        /// </summary>
        protected virtual void UpdateActivity(TEditViewModel model, TActivity activity)
        {
        }
    }
}
