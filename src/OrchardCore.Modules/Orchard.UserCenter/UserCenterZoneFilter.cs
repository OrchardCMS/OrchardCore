using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Orchard.UserCenter
{
    public class UserCenterZoneFilter : IActionFilter, IFilterMetadata
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                if(controller.GetType().Name.StartsWith("UserCenter"))
                {
                    UserCenterAttribute.Apply(context.HttpContext);
                }
            }
        }
    }
}
