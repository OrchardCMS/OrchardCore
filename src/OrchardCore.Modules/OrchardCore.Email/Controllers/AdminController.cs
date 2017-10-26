using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Models;
using OrchardCore.Email.Services;

namespace OrchardCore.Email.Controllers
{
    public class AdminController : Controller
    {
        private readonly IStringLocalizer<AdminController> T;
        private readonly IAuthorizationService _authorizationService;

        public AdminController(IStringLocalizer<AdminController> stringLocalizer,
            IAuthorizationService authorizationService)
        {
            T = stringLocalizer;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        public async Task<IActionResult> SmtpSettingsTest(TestSmtpOptions testSettings)
        {
            if ( !await _authorizationService.AuthorizeAsync( User, Permissions.ManageEmailSettings ) )
            {
                return Unauthorized();
            }

            var logger = new FakeLogger<SmtpService>();

            if ( !ModelState.IsValid )
            {
                logger.LogError( "Invalid settings" );
                return Json( new { error = logger.Message } );
            }

            var smtp = new SmtpService( Options.Create<SmtpSettings>( testSettings ), logger );

            try
            {
                await smtp.SendAsync( new EmailMessage
                {
                    From = testSettings.From,
                    Recipients = testSettings.To,
                    Cc = testSettings.Cc,
                    Bcc = testSettings.Bcc,
                    ReplyTo = testSettings.ReplyTo,
                    Subject = testSettings.Subject,
                    Body = testSettings.Body,
                } );


                if ( !String.IsNullOrEmpty( logger.Message ) )
                {
                    return Json( new { error = logger.Message } );
                }

                return Json( new { status = T["Message sent."].Value } );
            }
            catch ( Exception e )
            {
                return Json( new { error = e.Message } );
            }

        }


        private class FakeLogger<T> : ILogger<T>
        {
            public string Message { get; set; }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if ( formatter != null )
                {
                    Message = formatter( state, exception );
                }
                else
                {
                    Message = exception.Message;
                }
            }
        }

        public class TestSmtpOptions : SmtpSettings
        {
            public string From { get; set; }
            public string ReplyTo { get; set; }
            public string To { get; set; }
            public string Cc { get; set; }
            public string Bcc { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }
    }
}
