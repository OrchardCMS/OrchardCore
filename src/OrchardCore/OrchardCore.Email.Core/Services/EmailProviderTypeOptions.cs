using System;

namespace OrchardCore.Email.Services;

public class EmailProviderTypeOptions
{
    public Type Type { get; }

    public EmailProviderTypeOptions(Type type)
    {
        if (!typeof(IEmailProvider).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type must implement the '{nameof(IEmailProvider)}' interface.");
        }

        Type = type;
    }

    public bool IsEnabled { get; set; }
}
