using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public abstract class ContentEventDisplayDriver<TActivity, TViewModel> : ActivityDisplayDriver<TActivity> where TActivity : ContentEvent where TViewModel : ContentEventViewModel<TActivity>
    {
        protected ContentEventDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            ContentDefinitionManager = contentDefinitionManager;
        }

        private string BaseShapeName => typeof(TActivity).Name;

        protected IContentDefinitionManager ContentDefinitionManager { get; }

        public override IDisplayResult Display(TActivity activity)
        {
            return Combine(
                Shape($"{BaseShapeName}_Fields_Thumbnail", activity).Location("Thumbnail", "Content"),
                Shape($"{BaseShapeName}_Fields_Design", activity).Location("Design", "Content")
            );
        }

        public override IDisplayResult Edit(TActivity activity)
        {
            return EditShape<TViewModel>($"{BaseShapeName}_Fields_Edit", activity).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(TActivity model, IUpdateModel updater)
        {
            var viewModel = new ContentCreatedEventViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix, x => x.SelectedContentTypeNames))
            {
                model.ContentTypeFilter = FilterContentTypesQuery(viewModel.SelectedContentTypeNames).ToList();
            }
            return Edit(model);
        }

        protected override ShapeResult Shape(string shapeType, TActivity activity)
        {
            return Shape(shapeType, shape =>
            {
                if (shapeType.EndsWith("Design"))
                {
                    var contentTypeDefinitions = ContentDefinitionManager.ListTypeDefinitions().ToDictionary(x => x.Name);
                    var selectedContentTypeDefinitions = activity.ContentTypeFilter.Select(x => contentTypeDefinitions[x]).ToList();

                    shape.ContentTypeFilter = selectedContentTypeDefinitions;
                }
                shape.Activity = activity;
            });
        }

        protected ShapeResult EditShape<TModel>(string shapeType, TActivity activity) where TModel : ContentEventViewModel<TActivity>
        {
            return Shape<TModel>(shapeType, model =>
            {
                var availableContentTypes = ContentDefinitionManager.ListTypeDefinitions().Select(x => new SelectListItem { Text = x.DisplayName, Value = x.Name }).OrderBy(x => x.Text).ToList();

                availableContentTypes.Insert(0, new SelectListItem { Text = "Any", Value = "" });

                model.Activity = activity;
                model.AvailableContentTypes = availableContentTypes;
                model.SelectedContentTypeNames = activity.ContentTypeFilter.ToList();
            });
        }

        /// <summary>
        /// Filters out any content type that doesn't exist.
        /// </summary>
        protected IEnumerable<string> FilterContentTypesQuery(IEnumerable<string> contentTypeNames)
        {
            var contentTypeDefinitions = ContentDefinitionManager.ListTypeDefinitions().ToDictionary(x => x.Name);
            return contentTypeNames.Where(x => !string.IsNullOrWhiteSpace(x) && contentTypeDefinitions.ContainsKey(x));
        }
    }
}
