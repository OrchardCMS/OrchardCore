using System;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Deployment.Editors
{
    public abstract class DeploymentStepDisplayDriver<TStep> : DisplayDriverBase, IDeploymentStepDisplayDriver where TStep : DeploymentStep, new()
    {
        public Type Type => typeof(TStep);

        public Task<IDisplayResult> BuildDisplayAsync(DeploymentStep deploymentStep, BuildDisplayContext context)
        {
            var step = deploymentStep as TStep;

            if (step == null)
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            return DisplayAsync(step, context);
        }

        public Task<IDisplayResult> BuildEditorAsync(DeploymentStep deploymentStep, BuildEditorContext context)
        {
            var step = deploymentStep as TStep;

            if (step == null)
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            return EditAsync(step, context);
        }

        public Task<IDisplayResult> UpdateEditorAsync(DeploymentStep deploymentStep, UpdateEditorContext context)
        {
            var step = deploymentStep as TStep;

            if (step == null)
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            var result = UpdateAsync(step, context.Updater, context);

            if (result == null)
            {
                return Task.FromResult<IDisplayResult>(null);
            }
            
            return result;
        }

        public virtual Task<IDisplayResult> DisplayAsync(TStep step, BuildDisplayContext context)
        {
            return Task.FromResult(Display(step, context));
        }

        public virtual IDisplayResult Display(TStep step, BuildDisplayContext context)
        {
            return Display(step);
        }

        public virtual IDisplayResult Display(TStep step)
        {
            return null;
        }

        public virtual Task<IDisplayResult> EditAsync(TStep step, BuildEditorContext context)
        {
            return Task.FromResult(Edit(step, context));
        }

        public virtual IDisplayResult Edit(TStep step, BuildEditorContext context)
        {
            return Edit(step);
        }

        public virtual IDisplayResult Edit(TStep part)
        {
            return null;
        }

        public virtual Task<IDisplayResult> UpdateAsync(TStep step, IUpdateModel updater, UpdateEditorContext context)
        {
            return UpdateAsync(step, context);
        }

        public virtual Task<IDisplayResult> UpdateAsync(TStep step, BuildEditorContext context)
        {
            return UpdateAsync(step, context.Updater);
        }

        public virtual Task<IDisplayResult> UpdateAsync(TStep step, IUpdateModel updater)
        {
            return Task.FromResult<IDisplayResult>(null);
        }

        public DeploymentStep Create()
        {
            return new TStep();
        }
    }
}
