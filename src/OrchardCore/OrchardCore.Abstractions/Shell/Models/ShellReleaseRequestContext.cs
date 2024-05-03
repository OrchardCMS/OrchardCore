namespace OrchardCore.Environment.Shell.Models;

public class ShellReleaseRequestContext
{
    public const string ShellScopeKey = nameof(ShellReleaseRequestContext);

    public bool Release { get; set; }
}
