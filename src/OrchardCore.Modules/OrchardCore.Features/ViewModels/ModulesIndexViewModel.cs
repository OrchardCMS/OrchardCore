using OrchardCore.DisplayManagement;
using OrchardCore.Features.Models;

namespace OrchardCore.Features.ViewModels;

public class ModulesIndexViewModel
{
    public bool InstallModules { get; set; }
    public IEnumerable<ModuleEntry> Modules { get; set; }

    public ModulesIndexOptions Options { get; set; }
    public IShape Pager { get; set; }
}

public class ModulesIndexOptions
{
    public string SearchText { get; set; }
}
