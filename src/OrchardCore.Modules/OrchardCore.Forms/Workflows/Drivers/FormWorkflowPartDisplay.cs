using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Workflows.Models;
using OrchardCore.Forms.Workflows.ViewModels;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Forms.Workflows.Drivers
{
    public class FormWorkflowPartDisplay : ContentPartDisplayDriver<FormWorkflowPart>
    {
        private readonly IWorkflowTypeStore _workflowTypeStore;

        public FormWorkflowPartDisplay(IWorkflowTypeStore workflowTypeStore)
        {
            _workflowTypeStore = workflowTypeStore;
        }

        public override IDisplayResult Edit(FormWorkflowPart part, BuildPartEditorContext context)
        {
            return Initialize<FormWorkflowPartEditViewModel>("FormWorkflowPart_Fields_Edit", async m =>
            {
                m.WorkflowTypeId = part.WorkflowTypeId;
                m.AvailableWorkflowTypes = await GetAvailableWorkflowTypesAsync(part.WorkflowTypeId);
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(FormWorkflowPart part, IUpdateModel updater)
        {
            var viewModel = new FormWorkflowPartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.WorkflowTypeId = viewModel.WorkflowTypeId;
            }

            return Edit(part);
        }

        private async Task<IList<SelectListItem>> GetAvailableWorkflowTypesAsync(string selectedWorkflowTypeId)
        {
            var workflowTypes = await _workflowTypeStore.ListAsync();
            var options = workflowTypes.OrderBy(x => x.Name).Select(x => new SelectListItem
            {
                Value = x.WorkflowTypeId,
                Text = x.Name,
                Selected = x.WorkflowTypeId == selectedWorkflowTypeId
            }).ToList();

            options.Insert(0, new SelectListItem());
            return options;
        }
    }
}