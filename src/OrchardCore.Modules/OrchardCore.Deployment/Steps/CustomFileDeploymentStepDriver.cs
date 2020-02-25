using System.Threading.Tasks;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Steps
{
    public class CustomFileDeploymentStepDriver : DisplayDriver<DeploymentStep, CustomFileDeploymentStep>
    {
        public override IDisplayResult Display(CustomFileDeploymentStep step)
        {
            return
                Combine(
                    View("CustomFileDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("CustomFileDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(CustomFileDeploymentStep step)
        {
            return Initialize<CustomFileDeploymentStepViewModel>("CustomFileDeploymentStep_Fields_Edit", model =>
            {
                model.FileContent = step.FileContent;
                model.FileName = step.FileName;
                model.RecipeName = step.RecipeName;
                model.DisplayName = step.DisplayName;
                model.Description = step.Description;
                model.Author = step.Author;
                model.WebSite = step.WebSite;
                model.Version = step.Version;
                model.IsSetupRecipe = step.IsSetupRecipe;
                model.Categories = step.Categories;
                model.Tags = step.Tags;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(CustomFileDeploymentStep step, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(step, Prefix, x => x.FileName, x => x.FileContent, x => x.RecipeName, x => x.DisplayName, x => x.Description, x => x.Author, x => x.WebSite, x => x.Version, x => x.IsSetupRecipe, x => x.Categories, x => x.Tags);

            return Edit(step);
        }
    }
}
