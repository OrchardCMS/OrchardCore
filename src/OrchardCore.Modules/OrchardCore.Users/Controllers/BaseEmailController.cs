using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.Encodings.Web;
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

        protected async Task<bool> SendEmailAsync(string email, string subject, IShape model)
        {
            var options = ControllerContext.HttpContext.RequestServices.GetRequiredService<IOptions<MvcViewOptions>>();

            // Just use the current context to get a view and then create a view context.
            var view = options.Value.ViewEngines
                    .Select(x => x.FindView(
                        ControllerContext,
                        ControllerContext.ActionDescriptor.ActionName, 
                        false)).FirstOrDefault()?.View;

            var displayContext = new DisplayContext()
            {
                ServiceProvider = ControllerContext.HttpContext.RequestServices,
                Value = model,
                ViewContext = new ViewContext(ControllerContext, view, ViewData, TempData, new StringWriter(), new HtmlHelperOptions())
            };

            var body = string.Empty;

            using (var sw = new StringWriter())
            {
                var htmlContent = await _displayManager.ExecuteAsync(displayContext);
                htmlContent.WriteTo(sw, HtmlEncoder.Default);
                body = sw.ToString();
            }

            var message = new MailMessage()
            {
                Body = body,
                IsBodyHtml = true,
                Subject = subject
            };

            message.To.Add(email);

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