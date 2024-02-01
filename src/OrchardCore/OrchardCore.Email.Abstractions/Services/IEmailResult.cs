using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Email.Services;

public interface IEmailResult
{
    IEnumerable<LocalizedString> Errors { get; }

    bool Succeeded { get; }
}
