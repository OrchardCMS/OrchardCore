using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Mvc.ActionConstraints;
using Microsoft.AspNet.Routing;

namespace Orchard.Mvc
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
            if(routeContext.HttpContext.Request.Method != "POST")
            {
                return false;
            }

            var value = routeContext.HttpContext.Request.Form[_submitButtonName];
            return !string.IsNullOrEmpty(value);
        }
    }
}
