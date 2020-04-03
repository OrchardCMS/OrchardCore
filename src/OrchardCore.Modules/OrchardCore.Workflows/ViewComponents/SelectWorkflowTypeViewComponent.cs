using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.ViewComponents
{
    public class SelectWorkflowTypeViewComponent : ViewComponent
    {
        private readonly IWorkflowTypeStore _contentDefinitionManager;

        public SelectWorkflowTypeViewComponent(IWorkflowTypeStore contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string selectedWorkflowTypeId, string htmlName)
        {
            var selections = await WorkflowTypeSelection.BuildAsync(_contentDefinitionManager, selectedWorkflowTypeId);

            var model = new SelectWorkflowTypeViewModel
            {
                HtmlName = htmlName,
                WorkflowTypeSelections = selections
            };

            return View(model);
        }
    }
}
