using Riok.Mapperly.Abstractions;

namespace OrchardCore.Email.Smtp.Services;

[Mapper]
public static partial class SmtpSettingsMapper
{
    public static partial SmtpOptions Map(SmtpSettings source);
}
