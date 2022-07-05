namespace OrchardCore.Environment.Shell.Removing;

public class ShellRemovingResult
{
    public string TenantName { get; set; }
    public bool Success => ErrorMessage == null;
    public string ErrorMessage { get; set; }
}
