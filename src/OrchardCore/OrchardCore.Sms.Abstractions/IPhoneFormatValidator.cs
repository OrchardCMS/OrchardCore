using System;

namespace OrchardCore.Sms;

[Obsolete("This interface has been deprecated, please use IPhoneNumberValidator instead.")]
public interface IPhoneFormatValidator
{
    bool IsValid(string phoneNumber);
}
