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
        private static string ThumbnailshapeType = $"{typeof(TActivity).Name}_Fields_Thumbnail";
        private static string DesignShapeType = $"{typeof(TActivity).Name}_Fields_Design";

        public override IDisplayResult Display(TActivity model)
        {
            return Combine(
                Shape(ThumbnailshapeType, new ActivityViewModel<TActivity>(model)).Location("Thumbnail", "Content"),
                Shape(DesignShapeType, new ActivityViewModel<TActivity>(model)).Location("Design", "Content")
            );
        }
    }

    /// <summary>
    /// Base class for activity drivers using a strongly typed view model.
    /// </summary>
    public abstract class ActivityDisplayDriver<TActivity, TEditViewModel> : ActivityDisplayDriver<TActivity> where TActivity : class, IActivity where TEditViewModel : class, new()
    {

        private static string EditShapeType = $"{typeof(TActivity).Name}_Fields_Edit";

        public override IDisplayResult Edit(TActivity model)
        {
            return Initialize(EditShapeType, (System.Func<TEditViewModel, Task>)(viewModel =>
            {
                return EditActivityAsync(model, viewModel);
            })).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(TActivity model, IUpdateModel updater)
        {
            var viewModel = new TEditViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                await UpdateActivityAsync(viewModel, model);
            }

            return Edit(model);
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
