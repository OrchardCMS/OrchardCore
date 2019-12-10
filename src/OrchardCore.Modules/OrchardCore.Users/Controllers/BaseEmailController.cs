using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.DisplayManagement;
using OrchardCore.Email;

namespace OrchardCore.Users.Controllers
{
    public abstract class BaseEmailController : Controller
    {
        private readonly ISmtpService _smtpService;
        private readonly IDisplayHelper _displayHelper;
        private readonly HtmlEncoder _htmlEncoder;

        public BaseEmailController(
            ISmtpService smtpService,
            IDisplayHelper displayHelper,
            HtmlEncoder htmlEncoder)
        {
            _smtpService = smtpService;
            _displayHelper = displayHelper;
            _htmlEncoder = htmlEncoder;
        }

        protected async Task<bool> SendEmailAsync(string email, string subject, IShape model)
        {
            var body = string.Empty;

            using (var sb = StringBuilderPool.GetInstance())
            {
                using (var sw = new StringWriter(sb.Builder))
                {
                    var htmlContent = await _displayHelper.ShapeExecuteAsync(model);
                    htmlContent.WriteTo(sw, _htmlEncoder);
                    body = sw.ToString();
                }
            }

            var message = new MailMessage()
            {
                To = email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

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
