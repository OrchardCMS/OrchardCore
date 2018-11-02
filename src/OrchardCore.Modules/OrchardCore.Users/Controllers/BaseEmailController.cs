using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Email;

namespace OrchardCore.Users.Controllers
{
    [Admin]
    public class BaseEmailController : Controller
    {
        private readonly ISmtpService _smtpService;
        private readonly IShapeFactory _shapeFactory;
        private readonly IHtmlDisplay _displayManager;

        public BaseEmailController(
            ISmtpService smtpService,
            IShapeFactory shapeFactory,
            IHtmlDisplay displayManager)
        {
            _smtpService = smtpService;
            _shapeFactory = shapeFactory;
            _displayManager = displayManager;
        }

        protected async Task<bool> SendEmailAsync(string email, string subject, object model, string viewName)
        {
            var options = ControllerContext.HttpContext.RequestServices.GetRequiredService<IOptions<MvcViewOptions>>();

            // Just use the current context to get a view and then create a view context.
            var view = options.Value.ViewEngines.Select(x => x.FindView(ControllerContext,
                ControllerContext.ActionDescriptor.ActionName, false)).FirstOrDefault()?.View;

            var displayContext = new DisplayContext()
            {
                ServiceProvider = ControllerContext.HttpContext.RequestServices,
                Value = await _shapeFactory.CreateAsync(viewName, model),
                ViewContext = new ViewContext(ControllerContext, view, ViewData, TempData, new StringWriter(), new HtmlHelperOptions())
            };
            var htmlContent = await _displayManager.ExecuteAsync(displayContext);

            var message = new MailMessage() { Body = WebUtility.HtmlDecode(htmlContent.ToString()), IsBodyHtml = true, Subject = subject };
            message.To.Add(email);

            // send email
            var result = await _smtpService.SendAsync(message);

            return result.Succeeded;
        }

        protected IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("~/");
            }
        }
    }
}