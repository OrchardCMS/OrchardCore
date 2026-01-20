using System;

namespace OrchardCore.Environment.Shell.Removing;

public class ShellRemovingContext
{
    private string _errorMessage;

    public ShellSettings ShellSettings { get; set; }
    public bool LocalResourcesOnly { get; set; }
    public bool FailedOnLockTimeout { get; set; }
    public bool Success => _errorMessage == null;

    public string ErrorMessage
    {
        get => Error != null ? $"{_errorMessage} {Error.GetType().FullName}: {Error.Message}" : _errorMessage;

        set => _errorMessage = value;
    }

    public Exception Error { get; set; }
}
