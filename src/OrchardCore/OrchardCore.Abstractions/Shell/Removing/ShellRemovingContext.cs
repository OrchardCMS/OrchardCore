using System;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Environment.Shell.Removing;

public class ShellRemovingContext
{
    public ShellSettings ShellSettings { get; set; }
    public bool LocalResourcesOnly { get; set; }
    public bool FailedOnLockTimeout { get; set; }
    public bool Success => LocalizedErrorMessage == null;

    public LocalizedString LocalizedErrorMessage { get; set; }

    public string ErrorMessage => Error != null
        ? $"{LocalizedErrorMessage.Value} {Error.GetType().FullName}: {Error.Message}"
        : LocalizedErrorMessage.Value;

    public Exception Error { get; set; }
}
