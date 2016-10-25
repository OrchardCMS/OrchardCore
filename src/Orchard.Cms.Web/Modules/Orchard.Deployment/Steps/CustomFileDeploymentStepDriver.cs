using System;
using System.Threading.Tasks;
using Orchard.ContentManagement.MetaData;
using Orchard.Deployment.Editors;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Deployment.Steps
{
    public class CustomFileDeploymentStepDriver : DeploymentStepDisplayDriver<CustomFileDeploymentStep>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public CustomFileDeploymentStepDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(CustomFileDeploymentStep step)
        {
            return 
                Combine(
                    Shape("CustomFileDeploymentStep", step).Location("Summary", "Content"),
                    Shape("CustomFileDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(CustomFileDeploymentStep step)
        {
            return Shape("CustomFileDeploymentStep_Edit", step).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(CustomFileDeploymentStep step, IUpdateModel updater)
        {
            // Initializes the value to empty otherwise the model is not updated is no type is selected
            step.FileName = "";
            step.FileContent = "";

            await updater.TryUpdateModelAsync(step, Prefix, x => x.FileName, x => x.FileContent);

            return Edit(step);
        }
    }
}
