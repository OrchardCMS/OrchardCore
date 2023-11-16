using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Forms.Drivers
{
    public class FormPartDisplayDriver : ContentPartDisplayDriver<FormPart>
    {
        private readonly IServiceProvider _services;
        protected readonly IStringLocalizer S;

        public FormPartDisplayDriver(
            IServiceProvider services,
            IStringLocalizer<FormPartDisplayDriver> stringLocalizer)
        {
            _services = services;
            S = stringLocalizer;
        }

        public override IDisplayResult Edit(FormPart part)
        {
            return Initialize<FormPartEditViewModel>("FormPart_Fields_Edit", async m =>
            {
                m.Action = part.Action;
                m.Method = part.Method;
                m.WorkflowTypeId = part.WorkflowTypeId;
                m.EncType = part.EncType;
                m.EnableAntiForgeryToken = part.EnableAntiForgeryToken;
                m.SaveFormLocation = part.SaveFormLocation;
                m.WorkflowPayload = part.WorkflowPayload;

                m.WorkflowTypes.Insert(0, new SelectListItem() { Text = S["None"], Value = string.Empty });

                var workflowTypeStore = _services.GetService<IWorkflowTypeStore>();
                if (workflowTypeStore is not null)
                {
                    m.WorkflowTypes
                        .AddRange((await workflowTypeStore.GetByStartActivityAsync("HttpRequestEvent"))
                        .Select(wf => new SelectListItem() { Text = wf.Name, Value = wf.WorkflowTypeId }));
                }
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
                part.EncType = viewModel.EncType;
                part.EnableAntiForgeryToken = viewModel.EnableAntiForgeryToken;
                part.SaveFormLocation = viewModel.SaveFormLocation;

                part.WorkflowPayload = viewModel.WorkflowPayload;
                if (part.WorkflowPayload.WorkflowId is not null)
                {
                    var workflowTypeStore = _services.GetService<IWorkflowTypeStore>();
                    if (workflowTypeStore is not null)
                    {
                        part.WorkflowPayload.ActivityId = (await workflowTypeStore.GetAsync(part.WorkflowPayload.WorkflowId))
                            ?.Activities
                            .FirstOrDefault(a => string.Equals(a.Name, "HttpRequestEvent", StringComparison.OrdinalIgnoreCase))
                            ?.ActivityId;
                    }
                }
            }

            return Edit(part);
        }
    }
}
