using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace OrchardCore.Email
{
    public class SmtpSettings : IValidatableObject
    {
        [Required(AllowEmptyStrings = false), EmailAddress]
        public string DefaultSender { get; set; }

        [Required]
        public SmtpDeliveryMethod DeliveryMethod { get; set; }

        public string PickupDirectoryLocation { get; set; }

        public string Host { get; set; }
        [Range(0, 65535)]
        public int Port { get; set; } = 25;
        public bool EnableSsl { get; set; }
        public bool RequireCredentials { get; set; }
        public bool UseDefaultCredentials { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            switch (DeliveryMethod)
            {
                case SmtpDeliveryMethod.Network:
                    if (String.IsNullOrEmpty(Host))
                    {
                        yield return new ValidationResult("The Host name field is required.", new[] { nameof(Host) });
                    }
                    break;
                case SmtpDeliveryMethod.PickupDirectoryFromIis:
                    break;
                case SmtpDeliveryMethod.SpecifiedPickupDirectory:
                    if (String.IsNullOrEmpty(PickupDirectoryLocation))
                    {
                        yield return new ValidationResult("The Pickup directory location field is required.", new[] { nameof(PickupDirectoryLocation) });
                    }
                    break;
                default:
                    throw new NotSupportedException($"The '{DeliveryMethod}' delivery method is not supported.");
            }
        }
    }
}
