using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Secrets;
using OrchardCore.Workflows.Http.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Http.Drivers
{
    public class HttpRequestEventSecretDisplayDriver : DisplayDriver<Secret, HttpRequestEventSecret>
    {
        private readonly IStringLocalizer S;

        public HttpRequestEventSecretDisplayDriver(IStringLocalizer<HttpRequestEventSecretDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Display(HttpRequestEventSecret secret)
        {
            return View("HttpRequestEventSecret_Fields_Thumbnail", secret).Location("Thumbnail", "Content");
        }

        public override Task<IDisplayResult> EditAsync(HttpRequestEventSecret secret, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(Initialize<HttpRequestEventSecretViewModel>("HttpRequestEventSecret_Fields_Edit", model =>
            {
                model.WorkflowTypeId = secret.WorkflowTypeId;
                model.ActivityId = secret.ActivityId;
                model.TokenLifeSpan = secret.TokenLifeSpan;
                model.Token = secret.Token;
                model.Context = context;
            }).Location("Content"));
        }

        public override async Task<IDisplayResult> UpdateAsync(HttpRequestEventSecret secret, UpdateEditorContext context)
        {
            var model = new HttpRequestEventSecretViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                if (String.IsNullOrEmpty(model.WorkflowTypeId))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.WorkflowTypeId), S["The Workflow type id value is required"]);
                }

                secret.WorkflowTypeId = model.WorkflowTypeId;
                secret.ActivityId = model.ActivityId;
                secret.TokenLifeSpan = model.TokenLifeSpan;
                //TODO
                // if (model.RegenerateToken)
                // {
                    // var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);

                    // if (workflowType == null)
                    // {
                    //     return NotFound();
                    // }

                    // var token = _securityTokenService.CreateToken(new WorkflowPayload(workflowType.WorkflowTypeId, activityId), TimeSpan.FromDays(tokenLifeSpan == 0 ? NoExpiryTokenLifespan : tokenLifeSpan));
                    // var url = Url.Action("Invoke", "HttpWorkflow", new { token = token });


                //     // regenerate token
                // }
            }
                
            return await EditAsync(secret, context);
        }
    }
}
