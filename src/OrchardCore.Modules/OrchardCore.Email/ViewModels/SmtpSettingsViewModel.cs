using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Email.ViewModels;

public class SmtpSettingsViewModel : IValidatableObject
{
    public bool IsEnabled { get; set; }

    [Required(AllowEmptyStrings = false), EmailAddress]
    public string DefaultSender { get; set; }

    public string Host { get; set; }

    [Range(0, 65535)]
    public int Port { get; set; } = 25;

    public bool AutoSelectEncryption { get; set; }

    public bool RequireCredentials { get; set; }

    public bool UseDefaultCredentials { get; set; }

    public SmtpEncryptionMethod EncryptionMethod { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }

    public string ProxyHost { get; set; }

    public int ProxyPort { get; set; }

    public bool IgnoreInvalidSslCertificate { get; set; }

    [Required]
    public SmtpDeliveryMethod DeliveryMethod { get; set; }

    public string PickupDirectoryLocation { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var S = validationContext.GetService<IStringLocalizer<SmtpSettingsViewModel>>();

        switch (DeliveryMethod)
        {
            case SmtpDeliveryMethod.Network:
                if (string.IsNullOrEmpty(Host))
                {
                    yield return new ValidationResult(S["The {0} field is required.", "Host name"], new[] { nameof(Host) });
                }
                break;
            case SmtpDeliveryMethod.SpecifiedPickupDirectory:
                if (string.IsNullOrEmpty(PickupDirectoryLocation))
                {
                    yield return new ValidationResult(S["The {0} field is required.", "Pickup directory location"], new[] { nameof(PickupDirectoryLocation) });
                }
                break;
            default:
                throw new NotSupportedException(S["The '{0}' delivery method is not supported.", DeliveryMethod]);
        }
    }
}
