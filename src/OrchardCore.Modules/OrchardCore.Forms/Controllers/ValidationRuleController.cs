using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Forms.Helpers;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Controllers
{
    [Route("api/validationApi")]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    [ApiController]
    public class ValidationApiController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ValidationRuleHelpers _validationRuleHelpers;
        private readonly IStringLocalizer S;
        public ValidationApiController(
            IAuthorizationService authorizationService,
            ValidationRuleHelpers validationRuleHelpers,
            IStringLocalizer<ValidationApiController> stringLocalizer)
        {
            _authorizationService = authorizationService;
            _validationRuleHelpers = validationRuleHelpers;
            S = stringLocalizer;
        }
        [HttpGet]
        [Route("ValidateFormByRule")]
        public async Task<IActionResult> ValidateFormByRule(string contentItemId, string formName, string formValue)
        {
            if (String.IsNullOrWhiteSpace(contentItemId) || String.IsNullOrWhiteSpace(formName))
            {
                return BadRequest(S["contentItemId, formName are required parameters"]);
            }

            //if (!await _authorizationService.AuthorizeAsync(User, Permissions.ValidationRule))
            //{
            //    return Forbid();
            //}

            if (string.IsNullOrEmpty(formValue))
            {
                formValue = string.Empty;
            }

            var validationRuleService = HttpContext.RequestServices.GetService<IValidationRuleService>();
            var flowParts = await _validationRuleHelpers.GeFlowPartFromContentItemId(contentItemId);

            foreach (var item in flowParts)
            {
                foreach (var formFlowWidget in item.Widgets)
                {
                    var inputName = String.Empty;
                    var inputPart = formFlowWidget.As<InputPart>();
                    var textAreaPart = formFlowWidget.As<TextAreaPart>();
                    inputName = inputPart != null ? inputPart.ContentItem.DisplayText : textAreaPart != null ? textAreaPart.ContentItem.DisplayText : "";
                    if (inputName == null) continue;
                    if (formName != null && inputName != formName) continue;

                    var validationRulePart = formFlowWidget.Get(typeof(ValidationRulePart), nameof(ValidationRulePart)) as ValidationRulePart;
                    if (validationRulePart == null) continue;

                    var model = new ValidationRuleInput
                    {
                        Option = validationRulePart.Option,
                        Input = formValue,
                        Type = validationRulePart.Type,
                    };
                    var  validationResult = await validationRuleService.ValidateInputByRuleAsync(model);

                    if (!validationResult)
                    {
                        return Ok(new { errorMessage = S[validationRulePart.ErrorMessage ?? $"Validation failed for {validationRulePart.Type}."] }) ;
                    }
                }
            }

            return Ok();
        }
    }
}
