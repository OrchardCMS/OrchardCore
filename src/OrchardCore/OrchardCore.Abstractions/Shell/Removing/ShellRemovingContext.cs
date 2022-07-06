using System;

namespace OrchardCore.Environment.Shell.Removing;

public class ShellRemovingContext
{
    public ShellRemovingContext(string tenant) => TenantName = tenant;
    public string TenantName { get; set; }
    public bool Success => ErrorMessage == null;
    public string ErrorMessage { get; set; }
}
