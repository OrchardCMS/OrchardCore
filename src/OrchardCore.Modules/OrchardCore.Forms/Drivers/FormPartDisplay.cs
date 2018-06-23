using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Forms.Drivers
{
    public class FormPartDisplay : ContentPartDisplayDriver<FormPart>
    {
        private readonly IWorkflowTypeStore _workflowTypeStore;

        public FormPartDisplay(IWorkflowTypeStore workflowTypeStore)
        {
            _workflowTypeStore = workflowTypeStore;
        }

        public override IDisplayResult Edit(FormPart part, BuildPartEditorContext context)
        {
            return Initialize<FormPartEditViewModel>("FormPart_Fields_Edit", async m =>
            {
                m.Action = part.Action;
                m.Method = part.Method;
                m.WorkflowTypeId = part.WorkflowTypeId;
                m.AvailableWorkflowTypes = await GetAvailableWorkflowTypesAsync(part.WorkflowTypeId);
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(FormPart part, IUpdateModel updater)
        {
            var viewModel = new FormPartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.Action = viewModel.Action?.Trim();
                part.Method = viewModel.Method;
                part.WorkflowTypeId = viewModel.WorkflowTypeId;
            }

            return Edit(part);
        }

        private async Task<IList<SelectListItem>> GetAvailableWorkflowTypesAsync(string selectedWorkflowTypeId)
        {
            var workflowTypes = await _workflowTypeStore.ListAsync();
            return workflowTypes.OrderBy(x => x.Name).Select(x => new SelectListItem
            {
                Value = x.WorkflowTypeId,
                Text = x.Name,
                Selected = x.WorkflowTypeId == selectedWorkflowTypeId
            }).ToList();
        }
    }
}