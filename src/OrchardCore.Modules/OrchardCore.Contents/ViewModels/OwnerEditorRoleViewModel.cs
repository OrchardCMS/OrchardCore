using System;

namespace OrchardCore.Contents.ViewModels;

public class OwnerEditorRoleViewModel
{
    public bool SiteOwnerOnly { get; set; }

    public string[] Roles { get; set; } = Array.Empty<string>();
}
