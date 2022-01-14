using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Email
{
    public static class SmtpServiceExtensions
    {
        public static async Task<bool> SendAsync(this ISmtpService smtpService, string email, string subject, IShape model)
        {
            var body = String.Empty;
            var displayHelper = ShellScope.Services.GetService<IDisplayHelper>();
            var htmlEncoder = ShellScope.Services.GetService<HtmlEncoder>();

            using (var sw = new StringWriter())
            {
                var htmlContent = await displayHelper.ShapeExecuteAsync(model);
                htmlContent.WriteTo(sw, htmlEncoder);
                body = sw.ToString();
            }

            var message = new MailMessage()
            {
                To = email,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            var result = await smtpService.SendAsync(message);

            return result.Succeeded;
        }
    }
}
