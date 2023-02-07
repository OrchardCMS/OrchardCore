using System.Collections.Generic;

namespace OrchardCore.Contents.ViewModels;

public class RoleCommonPartSettingsViewModel
{
    public bool SiteOwnerOnly { get; set; } = true;

    public IList<RoleEntry> RoleItems { get; set; }
}

public class RoleEntry
{
    public string RoleName { get; set; }

    public bool IsSelected { get; set; }
}
