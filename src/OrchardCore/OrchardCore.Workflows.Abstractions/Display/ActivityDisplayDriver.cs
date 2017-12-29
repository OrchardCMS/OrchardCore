using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Abstractions.Display
{
    /// <summary>
    /// A display driver for <see cref="IActivity"/> types. Will add useful information to the created shape, such as the <see cref="IActivity"/> itself.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ActivityDisplayDriver<T> : DisplayDriver<IActivity, T> where T : class, IActivity
    {
        protected virtual ShapeResult Shape(string shapeType, T activity)
        {
            return Shape(shapeType, shape =>
            {
                shape.Activity = activity;
            });
        }
    }

    /// <summary>
    /// A convention-based activity display driver that simplifies its sub-classes.
    /// </summary>
    public abstract class ActivityDisplayDriver<TActivity, TActivityViewModel> : ActivityDisplayDriver<TActivity> where TActivity : class, IActivity where TActivityViewModel : class, new()
    {
        protected virtual string ShapeNameBase => typeof(TActivity).Name;

        public override IDisplayResult Display(TActivity activity)
        {
            return Combine(
                Shape($"{ShapeNameBase}_Fields_Thumbnail", activity).Location("Thumbnail", "Content"),
                Shape($"{ShapeNameBase}_Fields_Design", activity).Location("Design", "Content")
            );
        }

        public override IDisplayResult Edit(TActivity activity)
        {
            return Shape<TActivityViewModel>($"{ShapeNameBase}_Fields_Edit", model =>
            {
                Map(activity, model);
            }).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(TActivity activity, IUpdateModel updater)
        {
            var viewModel = CreateViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                Map(viewModel, activity);
            }
            return Edit(activity);
        }

        protected TActivityViewModel CreateViewModel()
        {
            return new TActivityViewModel();
        }

        /// <summary>
        /// Maps activity properties onto the view model. Called when the editor is to be displayed.
        /// </summary>
        protected virtual void Map(TActivity source, TActivityViewModel target)
        {
        }

        /// <summary>
        /// Maps view model properties onto the activity. Called when the editor form has been submitted back.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        protected virtual void Map(TActivityViewModel source, TActivity target)
        {
        }
    }
}
