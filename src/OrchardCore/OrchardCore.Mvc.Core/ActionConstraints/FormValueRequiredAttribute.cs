using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Mvc.ActionConstraints
{
    public class FormValueRequiredAttribute : ActionMethodSelectorAttribute
    {
        private readonly string _submitButtonName;

        public FormValueRequiredAttribute(string submitButtonName)
        {
            _submitButtonName = submitButtonName;
        }

        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            if (!HttpMethods.IsPost(routeContext.HttpContext.Request.Method))
            {
                return false;
            }

            var value = routeContext.HttpContext.Request.Form[_submitButtonName];
            return !string.IsNullOrEmpty(value);
        }
    }
}
