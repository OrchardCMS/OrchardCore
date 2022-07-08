using System;

namespace OrchardCore.Environment.Shell.Removing;

public class ShellRemovingContext
{
    public ShellSettings ShellSettings { get; set; }
    public bool Success => ErrorMessage == null;

    private string _errorMessage;
    public string ErrorMessage
    {
        get => Error != null
            ? $"{_errorMessage} {Error.GetType().FullName}: {Error.Message}"
            : _errorMessage;

        set => _errorMessage = value;
    }

    public Exception Error { get; set; }
}
