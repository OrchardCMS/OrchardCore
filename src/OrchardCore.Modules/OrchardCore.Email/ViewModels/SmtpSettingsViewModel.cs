using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Email.ViewModels
{
    public class SmtpSettingsViewModel : IValidatableObject
    {
        [Required(AllowEmptyStrings = false)]
        public string To { get; set; }

        public string Sender { get; set; }

        public string Bcc { get; set; }

        public string Cc { get; set; }

        public string ReplyTo { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var emailAddressValidator = validationContext.GetService<IEmailAddressValidator>();
            var S = validationContext.GetService<IStringLocalizer<SmtpSettingsViewModel>>();

            if (!String.IsNullOrWhiteSpace(Sender) && !emailAddressValidator.Validate(Sender))
            {
                yield return new ValidationResult(S["Invalid Email."], new[] { nameof(Sender) });
            }
        }
    }
}
