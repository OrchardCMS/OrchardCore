namespace OrchardCore.Environment.Shell.Removing;

public class ShellRemovingContext
{
    public ShellSettings ShellSettings { get; set; }
    public bool Success => ErrorMessage == null;
    public string ErrorMessage { get; set; }
}
